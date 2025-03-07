using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
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
        if (!builder.Configuration.GetValue<bool>("OpenTelemetry:enable"))
        {
            return builder;
        }
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        var isProduction = builder.Environment.IsProduction();
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
                metrics.AddEventCountersInstrumentation(opt =>
                    {
                        opt.RefreshIntervalSecs = 15;
                        opt.AddEventSources("Microsoft.EntityFrameworkCore");
                    })
                    .AddProcessInstrumentation()
                    .AddAspNetCoreInstrumentation()
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
                else if (isProduction)
                {
                    // 生产环境10%采样追踪
                    tracing.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.1)));
                }
                var groupName = "service.group";
                tracing.AddEntityFrameworkCoreInstrumentation(opt =>
                    {
                        opt.SetDbStatementForText = true;
                        opt.EnrichWithIDbCommand = (activity, command) =>
                            activity.SetTag(groupName, "database"); // 添加数据库操作分组
                    })
                    .AddRedisInstrumentation(opt =>
                    {
                        opt.Enrich = (activity, exception) =>
                            activity.SetTag(groupName, "cache"); // 添加缓存操作分组
                        if (!isProduction)
                        {
                            opt.SetVerboseDatabaseStatements = true;
                        }
                    })
                    .AddAspNetCoreInstrumentation(opt =>
                    {
                        opt.RecordException = true;
                        opt.EnrichWithHttpRequest = (activity, request) =>
                            activity.SetTag(groupName, "http-request"); // 添加HTTP请求分组
                        opt.EnrichWithHttpResponse = (activity, response) =>
                            activity.SetTag(groupName, "http-response"); // 添加HTTP响应分组
                        opt.EnrichWithException = (activity, exception) =>
                            activity.SetTag(groupName, "http-error"); // 添加HTTP错误分组
                    })
                    .AddGrpcClientInstrumentation(opt =>
                    {
                        opt.EnrichWithHttpRequestMessage = (activity, request) =>
                            activity.SetTag(groupName, "rpc-request"); // 添加RPC调用分组
                        opt.EnrichWithHttpResponseMessage = (activity, response) =>
                            activity.SetTag(groupName, "rpc-response"); // 添加RPC调用分组
                    })
                    .AddHttpClientInstrumentation(opt =>
                    {
                        opt.EnrichWithHttpRequestMessage = (activity, request) =>
                            activity.SetTag(groupName, "external-api-request"); // 添加外部API调用分组
                        opt.EnrichWithHttpResponseMessage = (activity, response) =>
                            activity.SetTag(groupName, "external-api-response"); // 添加外部API调用分组
                        opt.EnrichWithException = (activity, exception) =>
                            activity.SetTag(groupName, "external-api-error"); // 添加外部API调用分组
                    })
                    .AddSource("Experimental.Microsoft.Extensions.AI");
            });
        builder.AddOpenTelemetryExporters(isProduction);
        return builder;
    }

    /// <summary>
    /// 添加OpenTelemetry导出服务
    /// </summary>
    /// <param name="builder">宿主应用构建接口</param>
    /// <param name="isProduction">是否是生产环境</param>
    /// <returns>宿主应用构建接口</returns>
    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder, bool isProduction)
    {
        var exportProcessorType = isProduction? OpenTelemetry.ExportProcessorType.Batch : OpenTelemetry.ExportProcessorType.Simple;
        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter(opt =>
        {
            opt.ExportProcessorType = exportProcessorType;
        }));
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter(opt =>
        {
            opt.ExportProcessorType = exportProcessorType;
        }).AddPrometheusExporter());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter(opt =>
        {
            opt.ExportProcessorType = exportProcessorType;
        }));
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
        if (app.Configuration.GetValue<bool>("OpenTelemetry:enable"))
        {
            app.MapPrometheusScrapingEndpoint();
        }
        return app;
    }
}
