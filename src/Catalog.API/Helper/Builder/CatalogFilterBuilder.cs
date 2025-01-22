namespace eShop.Catalog.API.Helper.Builder;

/// <summary>
/// 目录筛选器构建类
/// </summary>
public class CatalogFilterBuilder : FilterBuilder<Models.Catalog>
{
    /// <summary>
    /// 添加目录类型id筛选条件
    /// </summary>
    /// <param name="typeId">目录类型id</param>
    /// <returns>目录筛选器构建类</returns>
    public CatalogFilterBuilder WithTypeId(int? typeId)
    {
        if (typeId.HasValue)
        {
            FilterExpression.Add(catalog => catalog.CatalogTypeId == typeId);
        }
        return this;
    }
    
    /// <summary>
    /// 添加目录品牌id筛选条件
    /// </summary>
    /// <param name="brandId">目录品牌id</param>
    /// <returns>目录筛选器构建类</returns>
    public CatalogFilterBuilder WithBrandId(int? brandId)
    {
        if (brandId.HasValue)
        {
            FilterExpression.Add(catalog => catalog.CatalogBrandId == brandId);
        }
        return this;
    }
}
