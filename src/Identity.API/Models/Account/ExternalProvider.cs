using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.Account;

/// <summary>
/// 外部提供类
/// </summary>
public class ExternalProvider
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 身份验证方案
    /// </summary>
    [Required]
    public required string AuthenticationScheme { get; set; }
}
