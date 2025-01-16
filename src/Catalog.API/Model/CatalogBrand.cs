namespace eShop.Catalog.API.Model;

/// <summary>
/// 目录品牌
/// </summary>
public class CatalogBrand : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }
}
