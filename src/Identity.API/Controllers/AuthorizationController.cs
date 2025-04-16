﻿using System.Security.Claims;
using Common.Helper;
using Identity.API.Models;
using Identity.API.Models.Account;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.API.Controllers;

/// <summary>
/// 授权控制器
/// </summary>
public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// 授权控制器构造函数
    /// </summary>
    /// <param name="applicationManager">应用管理</param>
    /// <param name="authorizationManager">授权管理</param>
    /// <param name="scopeManager">作用域管理</param>
    /// <param name="signInManager">登录管理</param>
    /// <param name="userManager">用户管理</param>
    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 认证
    /// </summary>
    /// <returns>认证结果</returns>
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        if (HttpMethods.IsPost(Request.Method) && !Request.HasFormContentType)
        {
            throw new InvalidOperationException("无效的请求内容类型");
        }
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("无法获取 OpenID 连接请求");
        // 修改表单键检查方式
        if (HttpMethods.IsPost(Request.Method) && Request.Form.TryGetValue("submit.Accept", out _))
        {
            request.ClientId.IsNotNull("客户端ID不存在");
            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("无法获取登录的用户信息");
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("找不到调用方客户端应用程序的详细信息");

            // 获取与用户和调用客户端应用程序关联的永久授权
            var userId = await _userManager.GetUserIdAsync(user);
            var clientId = await _applicationManager.GetIdAsync(application);
            clientId.IsNotNullOrWhitespace("客户端ID不存在");
            var scopes = request.GetScopes();
            var authorizations = await _authorizationManager.FindAsync(
                subject: userId,
                client: clientId,
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: scopes).ToListAsync();

            if (authorizations.Count is 0 && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "用户没有同意授权当前客户端应用程序的权限"
                    }));
            }

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                    .SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);

            identity.SetScopes(scopes);
            var identityScopes = identity.GetScopes();
            var resources = await _scopeManager.ListResourcesAsync(identityScopes).ToListAsync();
            identity.SetResources(resources);

            var authorization = authorizations.LastOrDefault();
            authorization ??= await _authorizationManager.CreateAsync(
                identity: identity,
                subject: userId,
                client: clientId!,
                type: AuthorizationTypes.Permanent,
                scopes: identity.GetScopes());

            identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (HttpMethods.IsPost(Request.Method) && Request.Form.TryGetValue("submit.Deny", out _))
        {
            return Deny();
        }
        else
        {
            var result = await HttpContext.AuthenticateAsync();
            if (result is null || !result.Succeeded || request.HasPromptValue(PromptValues.Login) ||
               (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                if (request.HasPromptValue(PromptValues.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "当前用户还未登录"
                        }));
                }

                var prompt = string.Join(" ", request.GetPromptValues().Remove(PromptValues.Login));

                var parameters = (Request.HasFormContentType ?
                    Request.Form.Where(parameter => parameter.Key != Parameters.Prompt) :
                    [.. Request.Query.Where(parameter => parameter.Key != Parameters.Prompt)]).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));
                return Challenge(
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters),
                        Items =
                        {
                            { "scheme", OpenIddictServerAspNetCoreDefaults.AuthenticationScheme }
                        }
                    },
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            }
            result.Principal.IsNotNull("用户认证信息不存在");
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("无法获取到用户信息");
            request.ClientId.IsNotNull("客户端ID不存在");
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("无法获取到客户端应用程序信息");

            var subjectInfo = await _userManager.GetUserIdAsync(user);
            var clientInfo = await _applicationManager.GetIdAsync(application);
            var scopesInfo = request.GetScopes();
            var authorizations = await _authorizationManager.FindAsync(subjectInfo, clientInfo, Statuses.Valid, AuthorizationTypes.Permanent, scopesInfo)
                .ToListAsync();
            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case ConsentTypes.External when authorizations.Count is 0:
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "当前登录用户没有操作当前客户端的权限"
                        }));
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Count is not 0:
                case ConsentTypes.Explicit when authorizations.Count is not 0 && !request.HasPromptValue(PromptValues.Consent):
                    var identity = new ClaimsIdentity(
                        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                        nameType: Claims.Name,
                        roleType: Claims.Role);

                    identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                            .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                            .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                            .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                            .SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);
                    var identityScopes = request.GetScopes();
                    identity.SetScopes(identityScopes);
                    identity.SetResources(await _scopeManager.ListResourcesAsync(identityScopes).ToListAsync());
                    var authorization = authorizations.LastOrDefault();
                    var clientId = await _applicationManager.GetIdAsync(application);
                    clientId.IsNotNullOrWhitespace("客户端ID不存在");
                    authorization ??= await _authorizationManager.CreateAsync(
                        identity: identity,
                        subject: await _userManager.GetUserIdAsync(user),
                        client: clientId!,
                        type: AuthorizationTypes.Permanent,
                        scopes: identityScopes);

                    identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                    identity.SetDestinations(GetDestinations);

                    return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
                case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "用户没有同意授权当前客户端应用程序的权限"
                        }));

                default: return View(new AuthorizeViewModel(await _applicationManager.GetLocalizedDisplayNameAsync(application), request.Scope));
            }
        }
    }

    /// <summary>
    /// 用户拒绝授权
    /// </summary>
    /// <returns>操作结果</returns>
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    /// <summary>
    /// 注销登录页面
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    /// <summary>
    /// 注销登录请求
    /// </summary>
    /// <returns>操作结果</returns>
    [ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();
        // 将用户注销的请求重定向到post_logout_redirect_uri指定的页面，如果未指定，则重定向到"/"
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    /// <summary>
    /// 交换令牌
    /// </summary>
    /// <returns>操作结果</returns>
    [IgnoreAntiforgeryToken]
    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> ExchangeToken()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("无法获取 OpenID 连接请求");
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
        {
            throw new InvalidOperationException("不支持当前授权请求");
        }
        // 获取存储在授权代码/刷新令牌中的声明主体
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        result.IsNotNull("用户认证信息不存在");
        result.Principal.IsNotNull("用户声明主体不存在");
        // 获取与授权代码/刷新令牌对应的用户配置文件
        var subject = result.Principal!.GetClaim(Claims.Subject);
        subject.IsNotNullOrWhitespace("用户ID不存在");
        var user = await _userManager.FindByIdAsync(subject!);
        if (user is null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "令牌无效或已过期"
                }));
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户已被锁定或禁用"
                }));
        }

        var identity = new ClaimsIdentity(result.Principal!.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // 获取用户的最新信息并设置到声明主体中
        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                .SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);
        identity.SetDestinations(GetDestinations);
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// 获取声明在令牌存储的位置
    /// </summary>
    /// <param name="claim">声明</param>
    /// <returns>令牌存储的位置</returns>
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        var existSubject = claim.Subject is not null;
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (existSubject && claim.Subject!.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;
                yield break;
            case Claims.Email:
                yield return Destinations.AccessToken;
                if (existSubject && claim.Subject!.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;
            case Claims.Role:
                yield return Destinations.AccessToken;
                if (existSubject && claim.Subject!.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;
            // 不要在令牌中存储敏感信息
            case "AspNet.Identity.SecurityStamp": yield break;
            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}