namespace Identity.API.Models.Account;

/// <summary>
/// 重定向视图模型
/// </summary>
public class RedirectViewModel
{
    /// <summary>
    /// 重定向链接
    /// </summary>
    public required string RedirectUrl { get; set; }
}
