namespace Identity.API.Models;

/// <summary>
/// 系统用户
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// 客户端id
    /// </summary>
    public required List<string> ClientIds { get; set; }
}
