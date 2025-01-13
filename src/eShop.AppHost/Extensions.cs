using Aspire.Hosting.Lifecycle;

namespace eShop.AppHost;

/// <summary>
/// 扩展类
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 添加一个钩子为所有应用程序添加 ASPNETCORE_FORWARDEDHEADERS_ENABLED 环境变量
    /// </summary>
    public static IDistributedApplicationBuilder AddForwardedHeaders(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    /// <summary>
    /// 添加转发头钩子
    /// </summary>
    private class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
        {
            foreach (var resource in appModel.GetProjectResources())
            {
                resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
                {
                    context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = "true";
                }));
            }
            return Task.CompletedTask;
        }
    }
}
