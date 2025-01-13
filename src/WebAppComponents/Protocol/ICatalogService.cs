using WebAppComponents.Catalog;

namespace WebAppComponents.Protocol;

/// <summary>
/// 目录服务接口
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// 获取目录数据 
    /// </summary>
    /// <param name="pageIndex">页数</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="brandId">品牌id</param>
    /// <param name="typeId">类型id</param>
    /// <returns>目录数据 </returns>
    Task<CatalogResult?> GetCatalogItemsAsync(int pageIndex, int pageSize, int? brandId, int? typeId);
    
    /// <summary>
    /// 获取目录品牌
    /// </summary>
    /// <returns>目录品牌</returns>
    Task<IEnumerable<CatalogBrand>> GetCatalogBrandsAsync();
    
    /// <summary>
    /// 获取目录类型
    /// </summary>
    /// <returns>目录类型</returns>
    Task<IEnumerable<CatalogType>> GetCatalogTypesAsync();
}
