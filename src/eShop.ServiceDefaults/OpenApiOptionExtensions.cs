using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace eShop.ServiceDefaults;

/// <summary>
/// OpenApi配置扩展
/// </summary>
public static class OpenApiOptionExtensions
{
    /// <summary>
    /// 应用Api版本信息
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <param name="title">标题</param>
    /// <param name="description">描述</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplyApiVersionInfo(this OpenApiOptions options, string title, string description)
    {
        options.AddDocumentTransformer((document, context, _) =>
        {
            var versionedDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            var apiDescription = versionedDescriptionProvider?.ApiVersionDescriptions
                .SingleOrDefault(versionDescription => versionDescription.GroupName == context.DocumentName);
            if (apiDescription == null)
                return Task.CompletedTask;
            document.Info.Version = apiDescription.ApiVersion.ToString();
            document.Info.Title = title;
            document.Info.Description = BuildDescription(apiDescription, description);
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 应用权限校验
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <param name="scopes">作用域</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplyAuthorizationChecks(this OpenApiOptions options, string[] scopes)
    {
        options.AddOperationTransformer((operation, context, _) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            if (!metadata.OfType<IAuthorizeData>().Any())
            {
                return Task.CompletedTask;
            }

            operation.Responses.TryAdd("401", new OpenApiResponse() { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse() { Description = "Forbidden" });

            var oAuthScheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };
            operation.Security = new List<OpenApiSecurityRequirement>() { new() { [oAuthScheme] = scopes } };
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 引用安全方案定义
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplySecuritySchemaDefinitions(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
        return options;
    }

    /// <summary>
    /// 引用安全方案定义
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplyOperationDeprecatedStatus(this OpenApiOptions options)
    {
        options.AddOperationTransformer((operation, context, _) =>
        {
            var apiDescription = context.Description;
            operation.Deprecated |= apiDescription.IsDeprecated();
            return Task.CompletedTask;
        });
        return options;
    }

    
    /// <summary>
    /// 引用安全方案定义
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplyApiVersionDescription(this OpenApiOptions options)
    {
        options.AddOperationTransformer((operation, _, _) =>
        {
            // 找到一个名为【api-version】的参数并为它添加一个描述
            var apiVersionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "api-version");
            if (apiVersionParameter is not null)
            {
                apiVersionParameter.Description = "Api版本，格式为：'major.minor'。";
                apiVersionParameter.Schema.Example = new OpenApiString("1.0");
            }
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 为方案中的可选属性的 Nullable 设置为false
    /// </summary>
    /// <param name="options">OpenApi配置</param>
    /// <returns>OpenApi配置</returns>
    public static OpenApiOptions ApplySchemaNullableFalse(this OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, _, _) =>
        {
            if (schema.Properties is not null)
            {
                foreach (var property in schema.Properties)
                {
                    if (schema.Required?.Contains(property.Key) != true)
                    {
                        property.Value.Nullable = false;
                    }
                }
            }
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 构建Api描述
    /// </summary>
    /// <param name="apiDescription">Api描述类</param>
    /// <param name="description">描述</param>
    /// <returns>Api描述</returns>
    private static string BuildDescription(ApiVersionDescription apiDescription, string description)
    {
        var resultBuilder = new StringBuilder();
        if (apiDescription.IsDeprecated)
        {
            if (resultBuilder.Length > 0)
            {
                if (resultBuilder[^1] != '.')
                {
                    resultBuilder.Append('.');
                }

                resultBuilder.Append(' ');
            }

            resultBuilder.Append("API版本已被弃用。");
        }

        if (apiDescription.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                if (resultBuilder.Length > 0)
                {
                    resultBuilder.Append(' ');
                }

                resultBuilder.Append($"API将在{when.Date.ToShortDateString()}弃用。").Append('.');
            }

            if (policy.HasLinks)
            {
                resultBuilder.AppendLine();
                var rendered = false;
                foreach (var link in policy.Links.Where(link => link.Type == "text/html"))
                {
                    if (!rendered)
                    {
                        resultBuilder.Append("<h4>链接</h4><ul>");
                        rendered = true;
                    }

                    var linkTitle = StringSegment.IsNullOrEmpty(link.Title)
                        ? link.LinkTarget.OriginalString
                        : link.Title.ToString();
                    resultBuilder.Append($"<li><a href=\"{link.LinkTarget.OriginalString}\"{linkTitle}</a></li>");
                }

                if (rendered)
                {
                    resultBuilder.Append("</ul>");
                }
            }
        }

        return resultBuilder.ToString();
    }

    /// <summary>
    /// 安全方案定义
    /// </summary>
    /// <param name="configuration">配置</param>
    private class SecuritySchemeDefinitionsTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
    {
        /// <summary>
        /// 转换指定的OpenApi接口
        /// </summary>
        /// <param name="document">OpenApi文档</param>
        /// <param name="context">OpenApi文档转换上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务</returns>
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var identitySection = configuration.GetSection("Identity");
            if (!identitySection.Exists())
            {
                return Task.CompletedTask;
            }

            var identityUrlExternal = identitySection.GetRequiredValue("Url");
            var scopes = identitySection.GetRequiredSection("Scopes").GetChildren()
                .ToDictionary(kvPair => kvPair.Key, kvPair => kvPair.Value);
            var securityScheme = new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                        TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                        Scopes = scopes
                    }
                }
            };
            document.Components ??= new();
            document.Components.SecuritySchemes.Add("oauth2", securityScheme);
            return Task.CompletedTask;
        }
    }
}
