namespace Identity.API.Protocols;

/// <summary>
/// 重定向服务接口
/// </summary>
public interface IRedirectService
{
    /// <summary>
    /// 从返回链接中提取重定向资源地址
    /// </summary>
    /// <param name="returnUrl">返回链接</param>
    /// <returns>重定向资源地址</returns>
    string ExtractRedirectUriFromReturnUrl(string returnUrl);
}
