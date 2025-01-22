using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.API.Configuration;

/// <summary>
/// 配置类
/// </summary>
public class Config
{
    /// <summary>
    /// Token有效期（小时）
    /// </summary>
    public static int TokenLifeTimeInHours { get; } = 2;
    
    /// <summary>
    /// Token有效期（秒）
    /// </summary>
    public static int TokenLifeTimeInSeconds => TokenLifeTimeInHours * 3600;
    
    /// <summary>
    /// 获取Api资源
    /// </summary>
    /// <returns>Api资源集合</returns>
    public static IEnumerable<ApiResource> GetApiResources()
    {
        return
        [
            new ApiResource("order", "Order Service")
        ];
    }

    /// <summary>
    /// 获取Api作用域
    /// </summary>
    /// <returns>Api作用域集合</returns>
    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return
        [
            new ApiScope("order", "Order Service")
        ];
    }

    /// <summary>
    /// 获取认证资源
    /// </summary>
    /// <returns>认证资源集合</returns>
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];
    }

    /// <summary>
    /// 获取客户端想要操作的资源
    /// </summary>
    /// <param name="configuration">配置</param>
    /// <returns>客户端可操作资源集合</returns>
    public static IEnumerable<Client> GetClientResources(IConfiguration configuration)
    {
        var secret = "secret".Sha256();
        var webAppClientUri = configuration["WebAppClient"];
        return
        [
            new Client
            {
                ClientId = "webApp",
                ClientName = "Web App Client",
                ClientSecrets =
                [
                    new Secret(secret)
                ],
                ClientUri = $"{webAppClientUri}",
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = false,
                RequireConsent = false,
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                RequirePkce = false,
                RedirectUris = [
                    $"{webAppClientUri}/signin-oidc"
                ],
                PostLogoutRedirectUris = [
                    $"{webAppClientUri}/signout-callback-oidc"
                ],
                AllowedScopes = [
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "order"
                ],
                AccessTokenLifetime = TokenLifeTimeInSeconds,
                IdentityTokenLifetime = TokenLifeTimeInSeconds,
            }
        ];
    }
}
