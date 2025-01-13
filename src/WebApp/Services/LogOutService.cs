using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebApp.Services;

/// <summary>
/// 注销服务
/// </summary>
public class LogOutService
{
    /// <summary>
    /// 注销登录
    /// </summary>
    /// <param name="httpContext">http请求上下文</param>
    public async Task LogOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}
