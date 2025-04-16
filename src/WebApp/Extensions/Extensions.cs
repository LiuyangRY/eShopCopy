using eShop.WebApp.Services;
using eShop.WebAppComponents.Protocols;
using eShop.WebAppComponents.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

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
            .AddApiVersion(ServiceConstants.CatalogApiVersion)
            .AddAuthToken();
    }

    /// <summary>
    /// 添加身份认证服务
    /// </summary>
    /// <param name="builder">宿主服务构建类</param>
    private static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var identityUri = builder.Configuration.GetRequiredValue(ServiceConstants.IdentityApiUri);
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        })
        .AddAuthentication(options =>
        {
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "eshop.webapp.auth";
            options.LoginPath = "/account/login";
        })
        .AddOpenIdConnect(options => 
        {
            options.Authority = identityUri;
            options.ClientId = ServiceConstants.WebApp;
            options.ClientSecret = $"{ServiceConstants.WebApp}SecretKey";
            options.ResponseType = "code";
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.CallbackPath = new PathString("/signin-oidc");
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.SaveTokens = true;
        });
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        builder.Services.AddCascadingAuthenticationState();
    }
}
