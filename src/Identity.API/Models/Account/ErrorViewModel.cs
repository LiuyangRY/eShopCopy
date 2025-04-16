namespace Identity.API.Models.Account;

/// <summary>
/// 错误视图模型
/// </summary>
/// <param name="errorMessage">错误消息</param>
public class ErrorViewModel(string errorMessage)
{
    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; } = errorMessage;
}
