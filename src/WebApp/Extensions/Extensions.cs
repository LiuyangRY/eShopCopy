using eShop.WebApp.Services;
using eShop.WebAppComponents.Protocols;
using eShop.WebAppComponents.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.JsonWebTokens;

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
        builder.AddAuthenticationServices();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // 应用程序服务
        builder.Services.AddScoped<LogOutService>();
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // Http 和 Grpc 客户端注册
        builder.Services
            .AddHttpClient<CatalogService>(
                httpClient => httpClient.BaseAddress = new Uri(ServiceConstants.CatalogApiUrl))
            .AddApiVersion(ServiceConstants.CatalogApiVersion);
        // .AddAuthToken();
    }

    /// <summary>
    /// 添加身份认证服务
    /// </summary>
    /// <param name="builder">宿主服务构建类</param>
    private static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
        var identityUri = builder.Configuration.GetRequiredValue(ServiceConstants.IdentityApiUri);
        var callBackUri = builder.Configuration.GetRequiredValue(ServiceConstants.WebAppUri);
        var sessionCookieLifetimeMinutes = builder.Configuration.GetValue("SessionCookieLifetimeMinutes", 60);
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetimeMinutes))
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = identityUri;
                options.SignedOutRedirectUri = callBackUri;
                options.ClientId = ServiceConstants.WebAppId;
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
            });
        // Blazor身份认证服务
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        builder.Services.AddCascadingAuthenticationState();
    }
}
