using System.Text.RegularExpressions;
using Identity.API.Protocols;

namespace Identity.API.Services;

/// <summary>
/// 重定向服务
/// </summary>
public partial class RedirectService : IRedirectService
{
    /// <summary>
    /// 从返回链接中提取重定向资源地址
    /// </summary>
    /// <param name="returnUrl">返回链接</param>
    /// <returns>重定向资源地址</returns>
    public string ExtractRedirectUriFromReturnUrl(string returnUrl)
    {
        var decodeUrl = System.Net.WebUtility.HtmlDecode(returnUrl);
        var results = RedirectUriRegex().Split(decodeUrl);
        if (results.Length < 2)
            return string.Empty;
        var result = results[1];
        var splitKey = result.Contains("signin-oidc") ? "signin-oidc" : "scope";
        results = Regex.Split(result, splitKey);
        if (results.Length < 2)
        {
            return string.Empty;
        }

        result = results[0];
        return result.Replace("%3A", ":")
            .Replace("%2F", "/")
            .Replace("&", "");
    }

    [GeneratedRegex("redirect_uri=")]
    private static partial Regex RedirectUriRegex();
}
