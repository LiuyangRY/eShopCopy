using System.Text.Json.Serialization;
using Pgvector;

namespace eShop.Catalog.API.Models;

/// <summary>
/// 目录
/// </summary>
public class Catalog : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// 图片文件名称
    /// </summary>
    public required string PictureFileName { get; set; }
    
    /// <summary>
    /// 目录类型id
    /// </summary>
    public int CatalogTypeId { get; set; }
    
    /// <summary>
    /// 目录类型
    /// </summary>
    public required CatalogType CatalogType { get; set; }
    
    /// <summary>
    /// 品牌id
    /// </summary>
    public int CatalogBrandId { get; set; }
    
    /// <summary>
    /// 目录品牌
    /// </summary>
    public required CatalogBrand CatalogBrand { get; set; }
    
    /// <summary>
    /// 可用库存
    /// </summary>
    public int AvailableStock { get; set; }
    
    /// <summary>
    /// 补充库存阈值
    /// </summary>
    public int RestockThreshold { get; set; }
    
    /// <summary>
    /// 最大库存阈值
    /// </summary>
    public int MaxStockThreshold { get; set; }
    
    /// <summary>
    /// 可选地嵌入目录项的描述
    /// </summary>
    [JsonIgnore]
    public Vector? Embedding { get; set; }
    
    /// <summary>
    /// 是否可以重新下单
    /// </summary>
    public bool OnReorder { get; set; }
}
