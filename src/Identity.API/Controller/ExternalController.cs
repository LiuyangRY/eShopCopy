using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Identity.API.Models;
using Identity.API.Models.Account;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controller;

/// <summary>
/// 外部控制器
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class ExternalController : Microsoft.AspNetCore.Mvc.Controller
{
    /// <summary>
    /// 基本返回链接
    /// </summary>
    private const string BaseReturnUrl = "~/";
    
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<ExternalController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEventService _events;

    /// <summary>
    /// 外部控制器构造函数
    /// </summary>
    /// <param name="interaction">身份认证交互服务</param>
    /// <param name="logger">日志</param>
    /// <param name="userManager">用户管理</param>
    /// <param name="signInManager">登录管理</param>
    /// <param name="events">登录事件服务</param>
    public ExternalController(IIdentityServerInteractionService interaction, ILogger<ExternalController> logger,
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEventService events)
    {
        _interaction = interaction;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _events = events;
    }

    /// <summary>
    /// 启动到外部身份验证提供程序的往返
    /// </summary>
    /// <param name="scheme">方案</param>
    /// <param name="returnUrl">返回链接</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Challenge(string scheme, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            returnUrl = BaseReturnUrl;
        if (!Url.IsLocalUrl(returnUrl) && !_interaction.IsValidReturnUrl(returnUrl))
        {
            // 用户可能点击了恶意链接，需要被记录下来
            throw new Exception("无效的返回链接");
        }

        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items = { { "returnUrl", returnUrl }, { "scheme", scheme } },
        };
        return Challenge(props, scheme);
    }

    /// <summary>
    /// 外部认证后处理
    /// </summary>
    /// <returns>处理任务</returns>
    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (!result.Succeeded)
        {
            throw new Exception("外部认证错误");
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(claim => $"{claim.Type}:{claim.Value}");
            _logger.LogDebug("外部声明：{@claims}", externalClaims);
        }

        var userTuple = await FindUserFromExternalProviderAsync(result);
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);
        var principal = await _signInManager.CreateUserPrincipalAsync(userTuple.user);
        additionalLocalClaims.AddRange(principal.Claims);
        var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? userTuple.user.Id;
        var issuer = new IdentityServerUser(userTuple.user.Id)
        {
            DisplayName = name, IdentityProvider = userTuple.provider, AdditionalClaims = additionalLocalClaims
        };
        await HttpContext.SignInAsync(issuer, localSignInProps);
        // 删除外部认证使用的临时cookie
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        var returnUrl = result.Properties.Items["returnUrl"] ?? BaseReturnUrl;
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(userTuple.provider, userTuple.providerUserId,
            userTuple.user.Id, name, true, context?.Client.ClientId));
        if (context is not null && context.IsNativeClient())
        {
            return this.LoadingPage("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
        }

        return Redirect(returnUrl);
    }


    /// <summary>
    /// 从外部认证查找用户
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
        FindUserFromExternalProviderAsync(AuthenticateResult result)
    {
        var userIdClaim = result.Principal?.FindFirst(JwtClaimTypes.Subject) ??
                          result.Principal?.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("未知用户id");
        var claims = result.Principal.Claims.ToList();
        claims.Remove(userIdClaim);
        if (!(result.Properties?.Items.TryGetValue("scheme", out var provider) ?? false)
            || string.IsNullOrWhiteSpace(provider))
        {
            throw new Exception("外部登录提供方案不存在");
        }

        var providerUserId = userIdClaim.Value;
        var user = await _userManager.FindByLoginAsync(provider, providerUserId) ??
                   await AutoProvisionUserAsync(provider, providerUserId, claims);

        return (user, provider, providerUserId, claims);
    }

    /// <summary>
    /// 自动提供用户
    /// </summary>
    /// <param name="provider">外部认证方案</param>
    /// <param name="providerUserId">用户id</param>
    /// <param name="claims">用户拥有的声明</param>
    /// <returns>应用程序用户</returns>
    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId,
        List<Claim> claims)
    {
        List<Claim> filtered = [];
        var name = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Name)?.Value ??
                   claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
        if (!string.IsNullOrWhiteSpace(name))
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            var firstName = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.GivenName)?.Value ??
                            claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.FamilyName)?.Value ??
                           claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value;
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, $"{firstName} {lastName}"));
            }
            else if (!string.IsNullOrWhiteSpace(firstName))
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, firstName));
            }
            else if (!string.IsNullOrWhiteSpace(lastName))
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, lastName));
            }
        }

        var email = claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Email)?.Value ??
                    claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
        if (email is not null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        }

        var user = new ApplicationUser
        {
            UserName = Guid.NewGuid()
                .ToString(),
            CardNumber = string.Empty,
            SecurityNumber = string.Empty,
            Expiration = string.Empty,
            CardHolderName = string.Empty,
            Street = string.Empty,
            City = string.Empty,
            State = string.Empty,
            Country = string.Empty,
            ZipCode = string.Empty,
            Name = string.Empty,
            LastName = string.Empty
        };
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
            throw new Exception(identityResult.Errors.First().Description);
        if (filtered.Count > 0)
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded)
                throw new Exception(identityResult.Errors.First().Description);
        }

        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded)
            throw new Exception(identityResult.Errors.First().Description);
        return user;
    }

    /// <summary>
    /// 处理登录回调(如果外部登录是基于OIDC的，我们需要保存某些东西才能正常注销，这和WS-Fed、SAML2p或其他协议不同)
    /// </summary>
    /// <param name="result">身份验证结果</param>
    /// <param name="additionalLocalClaims">额外的本地标识</param>
    /// <param name="localSignInProps">本地登录属性</param>
    private void ProcessLoginCallback(AuthenticateResult result, List<Claim> additionalLocalClaims,
        AuthenticationProperties localSignInProps)
    {
        var sessionId = result.Principal?.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.SessionId)?.Value;
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            additionalLocalClaims.Add(new Claim(JwtClaimTypes.SessionId, sessionId));
        }

        var idToken = result.Properties?.GetTokenValue("id_token");
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            localSignInProps.StoreTokens([new AuthenticationToken() { Name = "id_token", Value = idToken }]);
        }
    }
}
