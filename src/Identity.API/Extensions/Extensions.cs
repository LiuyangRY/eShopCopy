using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Identity.API.Configuration;
using Identity.API.Infrastructure;
using Identity.API.Models;
using Identity.API.Models.Account;
using Identity.API.Protocols;
using Identity.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Extensions;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加应用程序服务
    /// </summary>
    /// <param name="webBuilder">Web应用程序构建类</param>
    /// <returns>Web应用程序构建类</returns>
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder webBuilder)
    {
        webBuilder.Services.AddControllersWithViews();
        webBuilder.AddNpgsqlDbContext<IdentityContext>("identityDb");
        if (webBuilder.Environment.IsDevelopment())
        {
            webBuilder.Services.AddMigration<IdentityContext, IdentityContextSeed>();
        }

        webBuilder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();
        webBuilder.Services.AddIdentityServer(options =>
            {
                options.Authentication.CookieLifetime = TimeSpan.FromHours(Config.TokenLifeTimeInHours);
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                if (webBuilder.Environment.IsDevelopment())
                {
                    options.KeyManagement.Enabled = false;
                }
            })
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiScopes(Config.GetApiScopes())
            .AddInMemoryApiResources(Config.GetApiResources())
            .AddInMemoryClients(Config.GetClientResources(webBuilder.Configuration))
            .AddAspNetIdentity<ApplicationUser>()
            // 在生产环境中在其他位置保存key
            .AddDeveloperSigningCredential();
        webBuilder.Services.AddTransient<IProfileService, ProfileService>();
        webBuilder.Services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
        webBuilder.Services.AddTransient<IRedirectService, RedirectService>();
        return webBuilder;
    }

    /// <summary>
    /// 检查重定向URI是否来自原生客户端
    /// </summary>
    /// <param name="request">请求</param>
    /// <returns>来自原生客户端返回true，否则返回false</returns>
    public static bool IsNativeClient(this AuthorizationRequest request)
    {
        return !request.RedirectUri.StartsWith("https", StringComparison.Ordinal) &&
               !request.RedirectUri.StartsWith("http", StringComparison.Ordinal);
    }

    /// <summary>
    /// 加载页面
    /// </summary>
    /// <param name="controller">控制器</param>
    /// <param name="viewName">视图名称</param>
    /// <param name="viewModel">视图模型</param>
    /// <returns>请求结果</returns>
    public static IActionResult LoadingPage<T>(this Microsoft.AspNetCore.Mvc.Controller controller, string viewName, T viewModel) where T : class
    {
        controller.HttpContext.Response.StatusCode = 200;
        controller.HttpContext.Response.Headers["Location"] = string.Empty;
        return controller.View(viewName, viewModel);
    }
}
