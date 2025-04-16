using Common.Constant;
using OpenIddict.Abstractions;

namespace Identity.API.Configuration;

/// <summary>
/// 配置类
/// </summary>
public class Config
{
    /// <summary>
    /// AccessToken有效期（小时）
    /// </summary>
    public static int AccessTokenLifeTimeInHours { get; } = 2;

    /// <summary>
    /// RefreshToken有效期（秒）
    /// </summary>
    public static int RefreshTokenLifeTimeInDays { get; } = 1;

    /// <summary>
    /// 获取作用域
    /// </summary>
    /// <returns>作用域数组</returns>
    public static string[] GetScopes() =>
    [
        OpenIddictConstants.Scopes.OpenId,
        OpenIddictConstants.Scopes.Email,
        OpenIddictConstants.Scopes.Profile,
        OpenIddictConstants.Scopes.Roles,
        OpenIddictConstants.Scopes.OfflineAccess,
        ServiceConstants.WebApp,
        ServiceConstants.ApiTest,
    ];
}
