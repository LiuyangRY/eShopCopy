using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace eShop.ServiceDefaults;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加默认服务
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <returns>宿主应用构建接口</returns>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddBasicServiceDefaults();
        builder.Services.AddServiceDiscovery();
        return builder;
    }


    /// <summary>
    /// 添加基本默认服务 
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <returns>宿主应用构建接口</returns>
    private static IHostApplicationBuilder AddBasicServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        builder.ConfigureOpenTelemetry();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // 开启可伸缩性
            http.AddStandardResilienceHandler();
            // 开启服务发现
            http.AddServiceDiscovery();
        });
        return builder;
    }

    /// <summary>
    /// 添加默认健康检查
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <returns>宿主应用构建接口</returns>
    private static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        return builder;
    }

    /// <summary>
    /// 配置OpenTelemetry
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <returns>宿主应用构建接口</returns>
    private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        if (builder.Configuration["OpenTelemetry:enable"] != "True")
        {
            return builder;
        } 
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: builder.Environment.ApplicationName)
                    .AddAttributes(new Dictionary<string, object>()
                    {
                        ["deployment.environment"] = builder.Environment.EnvironmentName,  // 添加环境标签
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddMeter("Experimental.Microsoft.Extensions.AI");
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // 开发环境全采样追踪
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Experimental.Microsoft.Extensions.AI");
            });
        builder.AddOpenTelemetryExporters();
        return builder;
    }

    /// <summary>
    /// 添加OpenTelemetry导出服务
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <returns>宿主应用构建接口</returns>
    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter().AddPrometheusExporter());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        return builder;
    }

    /// <summary>
    /// 配置应用程序默认终端点
    /// </summary>
    /// <param name="app">Web应用</param>
    /// <returns>Web应用</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }
        
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = (check) => check.Tags.Contains("live") });
        if (app.Configuration["OpenTelemetry:enable"] == "True")
        {
            app.MapPrometheusScrapingEndpoint();
        } 
        return app;
    }
}
