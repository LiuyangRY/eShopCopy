using Identity.API.Infrastructure;

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
        // webBuilder.AddNpgsqlDbContext<IdentityContext>("identityDb");
        return webBuilder;
    }
}
