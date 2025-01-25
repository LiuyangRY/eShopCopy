using System.Reflection.Metadata.Ecma335;
using Common.Helper;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Identity.API.Models;
using Identity.API.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controller;

/// <summary>
/// 主页控制器
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IClientStore _clientStore;
    private readonly IEventService _events;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// 主页控制器构造函数
    /// </summary>
    /// <param name="interaction">认证服务器交互服务</param>
    /// <param name="schemeProvider">方案提供类</param>
    /// <param name="clientStore">客户端配置</param>
    /// <param name="events">身份认证事件</param>
    /// <param name="signInManager">登录管理类</param>
    public AccountController(IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider, IClientStore clientStore, IEventService events,
        SignInManager<ApplicationUser> signInManager)
    {
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _clientStore = clientStore;
        _events = events;
        _signInManager = signInManager;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="returnUrl">返回链接</param>
    /// <returns>登录页</returns>
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        var viewModel = await BuildLoginViewModelAsync(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        if (viewModel.IsExternalLoginOnly)
        {
            return RedirectToAction("Challenge", "External", new { scheme = viewModel.ExternalLoginScheme, returnUrl });
        }

        return View(viewModel);
    }

    /// <summary>
    /// 用户名密码登录
    /// </summary>
    /// <param name="model">登录数据</param>
    /// <returns>登录结果页面</returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberLogin,
                lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(model.UserName);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user!.UserName, user.Id, user.UserName,
                    clientId: context?.Client.ClientId));
                if (context is not null)
                {
                    if (context.IsNativeClient())
                    {
                        return this.LoadingPage("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }

                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                if (string.IsNullOrWhiteSpace(model.ReturnUrl))
                {
                    return Redirect("~/");
                }

                throw new Exception("无效的返回链接");
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(model.UserName,
                AccountOptions.InvalidCredentialsErrorMessage, clientId: context?.Client.ClientId));
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
        }

        var viewModel = await BuildLoginViewModelAsync(model);
        ViewData["ReturnUrl"] = model.ReturnUrl;
        return View(viewModel);
    }

    /// <summary>
    /// 构建登录视图模型
    /// </summary>
    /// <param name="loginInputModel">登录输入模型</param>
    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel loginInputModel)
    {
        var viewModel = await BuildLoginViewModelAsync(loginInputModel.ReturnUrl);
        viewModel.UserName = loginInputModel.UserName;
        viewModel.RememberLogin = loginInputModel.RememberLogin;
        return viewModel;
    }

    /// <summary>
    /// 构建登录视图模型
    /// </summary>
    /// <param name="returnUrl">返回链接</param>
    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;
            var viewModel = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                UserName = context.LoginHint ?? string.Empty,
                Password = string.Empty
            };
            if (!local)
            {
                viewModel.ExternalProviders =
                [
                    new ExternalProvider() { AuthenticationScheme = context.IdP }
                ];
            }

            return viewModel;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();
        var providers = schemes.Where(scheme => scheme.DisplayName != null)
            .Select(scheme => new ExternalProvider
            {
                DisplayName = scheme.DisplayName ?? scheme.Name, AuthenticationScheme = scheme.Name
            }).ToList();
        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;
                if (client.IdentityProviderRestrictions?.Any() == true)
                {
                    providers = providers.Where(provider =>
                        client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                }
            }
        }

        return new LoginViewModel()
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            UserName = context?.LoginHint ?? string.Empty,
            Password = string.Empty,
            ExternalProviders = providers,
        };
    }
}
