using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace eShop.ServiceDefaults;

/// <summary>
/// OpenApi扩展
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// 使用OpenApi默认路由
    /// </summary>
    /// <param name="webApp">网站应用程序</param>
    /// <returns>应用程序构建类</returns>
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication webApp)
    {
        var openApiSection = webApp.Configuration.GetSection("OpenApi");
        if (!openApiSection.Exists())
        {
            return webApp;
        }

        webApp.MapOpenApi();
        if (webApp.Environment.IsDevelopment())
        {
            webApp.MapScalarApiReference(options =>
            {
                // 禁用默认字体以避免下载不必要的字体
                options.DefaultFonts = false;
            });
            webApp.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        }
        return webApp;
    }
    
    /// <summary>
    /// 添加默认OpenApi
    /// </summary>
    /// <param name="builder">宿主应用程序构建类</param>
    /// <param name="apiVersioningBuilder">Api版本构建类</param>
    /// <returns>宿主应用程序构建类</returns>
    public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder,
        IApiVersioningBuilder? apiVersioningBuilder = null)
    {
        if (apiVersioningBuilder is null)
        {
            return builder;
        }

        var openApi = builder.Configuration.GetSection("OpenApi");
        if (!openApi.Exists())
        {
            return builder;
        }

        var identitySection = builder.Configuration.GetSection("Identity");
        var scopes = identitySection.Exists()
            ? identitySection.GetRequiredSection("Scopes").GetChildren()
                .ToDictionary(kvPair => kvPair.Key, kvPair => kvPair.Value)
            : new Dictionary<string, string?>();
        // 默认格式是 ApiVersion.ToString()，例如：1.0。下面将版本格式化为 "'v'{主版本[.小版本][-状态]}"
        apiVersioningBuilder.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");
        string[] versions = ["v1"];
        foreach (var version in versions)
        {
            builder.Services.AddOpenApi(version, options =>
            {
                options.ApplyApiVersionInfo(openApi.GetRequiredValue("Document:Title"),
                    openApi.GetRequiredValue("Document:Description"));
                options.ApplyAuthorizationChecks([..scopes.Keys]);
                options.ApplySecuritySchemaDefinitions();
                options.ApplyOperationDeprecatedStatus();
                options.ApplyApiVersionDescription();
                options.ApplySchemaNullableFalse();
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Servers = [];
                    return Task.CompletedTask;
                });
            });
        }
        return builder;
    }
}
