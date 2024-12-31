using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.ServiceDefaults;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加默认服务
    /// </summary>
    /// <param name="builder">宿主应用构建类</param>
    /// <returns>宿主应用构建类</returns>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddBasicServiceDefaults();
        builder.Services.AddServiceDiscovery();
        return builder;
    }


    /// <summary>
    /// 添加基本默认服务 
    /// </summary>
    /// <param name="builder">宿主应用构建类</param>
    /// <returns>宿主应用构建类</returns>
    public static IHostApplicationBuilder AddBasicServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        return builder;
    }

    /// <summary>
    /// 添加默认健康检查
    /// </summary>
    /// <param name="builder">宿主应用构建类</param>
    /// <returns>宿主应用构建类</returns>
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        return builder;
    }

    /// <summary>
    /// 配置OpenTelemetry
    /// </summary>
    /// <param name="builder">宿主应用构建类</param>
    /// <returns>宿主应用构建类</returns>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry();
        return builder;
    }

    /// <summary>
    /// 配置应用程序默认终端点
    /// </summary>
    /// <param name="app">Web应用</param>
    /// <returns>Web应用</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("live"),
            });
        }
        return app;
    }
}
