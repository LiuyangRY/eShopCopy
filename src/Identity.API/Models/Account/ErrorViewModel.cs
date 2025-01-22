using Duende.IdentityServer.Models;

namespace Identity.API.Models.Account;

/// <summary>
/// 错误视图模型
/// </summary>
/// <param name="errorMessage">错误消息</param>
public class ErrorViewModel(ErrorMessage errorMessage)
{
    /// <summary>
    /// 错误消息
    /// </summary>
    public ErrorMessage ErrorMessage { get; set; } = errorMessage;
}
