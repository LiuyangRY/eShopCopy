namespace eShop.WebAppComponents.Model;

/// <summary>
/// 目录
/// </summary>
/// <param name="Id">目录id</param>
/// <param name="Name">目录名称</param>
/// <param name="Description">目录描述</param>
/// <param name="Price">目录价格</param>
/// <param name="PictureUrl">图片链接</param>
/// <param name="CatalogBrandId">目录品牌id</param>
/// <param name="CatalogBrand">目录品牌</param>
/// <param name="CatalogTypeId">目录类型id</param>
/// <param name="CatalogType">目录类型</param>
public record CatalogItem(int Id, string Name, string Description, decimal Price, string PictureUrl, int CatalogBrandId, CatalogBrand CatalogBrand, int CatalogTypeId, CatalogType CatalogType);
