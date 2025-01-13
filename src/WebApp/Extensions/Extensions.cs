using eShop.WebApp.Services;
using WebAppComponents.Protocol;
using WebAppComponents.Services;

namespace eShop.WebApp.Extensions;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加应用服务
    /// </summary>
    /// <param name="builder">宿主服务构建类</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpForwarderWithServiceDiscovery();
        
        // 应用程序服务
        builder.Services.AddScoped<LogOutService>();
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // Http 和 Grpc 客户端注册
        builder.Services.AddHttpClient<CatalogService>(httpClient => httpClient.BaseAddress = new Uri("http://catalog-api"))
            .AddApiVersion(1.0);
            //.AddAuthToken();
    }
}
