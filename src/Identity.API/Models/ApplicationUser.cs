using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models;

/// <summary>
/// 系统用户
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// 卡片号码
    /// </summary>
    [Required]
    public required string CardNumber { get; set; }
    
    /// <summary>
    /// 安全码
    /// </summary>
    [Required]
    public required string SecurityNumber { get; set; }
    
    /// <summary>
    /// 过期时间
    /// </summary>
    [Required]
    [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "过期时间的格式必须为：{MM/YY}")]
    public required string Expiration { get; set; }
    
    /// <summary>
    /// 持卡人名称
    /// </summary>
    [Required]
    public required string CardHolderName { get; set; }
    
    /// <summary>
    /// 卡片类型
    /// </summary>
    [Required]
    public int CardType { get; set; }
    
    /// <summary>
    /// 街道
    /// </summary>
    [Required]
    public required string Street { get; set; }
    
    /// <summary>
    /// 城市
    /// </summary>
    [Required]
    public required string City { get; set; }
    
    /// <summary>
    /// 州
    /// </summary>
    [Required]
    public required string State { get; set; }
    
    /// <summary>
    /// 国家
    /// </summary>
    [Required]
    public required string Country { get; set; }
    
    /// <summary>
    /// 邮政编码
    /// </summary>
    [Required]
    public required string ZipCode { get; set; }
    
    /// <summary>
    /// 姓
    /// </summary>
    [Required]
    public required string Name { get; set; }
    
    /// <summary>
    /// 名
    /// </summary>
    [Required]
    public required string LastName { get; set; }
}
