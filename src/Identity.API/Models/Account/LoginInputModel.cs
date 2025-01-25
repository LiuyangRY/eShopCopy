using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.Account;

/// <summary>
/// 登录输入模型
/// </summary>
public class LoginInputModel
{
    /// <summary>
    /// 用户名称
    /// </summary>
    [Required]
    public required string UserName { get; set; }
    
    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public required string Password { get; set; }
    
    /// <summary>
    /// 是否记住登录状态
    /// </summary>
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 返回链接
    /// </summary>
    public string ReturnUrl { get; set; } = "~/";
}
