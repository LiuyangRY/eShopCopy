using Identity.API.Models;
using Identity.API.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// 主页控制器构造函数
    /// </summary>
    public AccountController(
        IAuthenticationSchemeProvider schemeProvider,
        SignInManager<ApplicationUser> signInManager)
    {
        _schemeProvider = schemeProvider;
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
        if (!ModelState.IsValid)
        {
            throw new Exception("无效的返回链接");
        }
        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberLogin,
                lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var user = await _signInManager.UserManager.FindByNameAsync(model.UserName);
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
        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);

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
        var schemes = await _schemeProvider.GetAllSchemesAsync();
        var providers = schemes.Where(scheme => scheme.DisplayName != null)
            .Select(scheme => new ExternalProvider
            {
                DisplayName = scheme.DisplayName ?? scheme.Name,
                AuthenticationScheme = scheme.Name
            }).ToList();
        var allowLocal = true;

        return new LoginViewModel()
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            UserName = string.Empty,
            Password = string.Empty,
            ExternalProviders = providers,
        };
    }
}
