namespace eShop.Catalog.API.Model;

/// <summary>
/// 数据库基本实体
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// id
    /// </summary>
    public required int Id { get; set; }
    
    /// <summary>
    /// 创建人
    /// </summary>
    public required int CreatedBy { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public required DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 修改人
    /// </summary>
    public required int UpdatedBy { get; set; }
    
    /// <summary>
    /// 修改时间
    /// </summary>
    public required DateTime UpdatedAt { get; set; }
}
