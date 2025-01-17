using eShop.WebAppComponents.Model;

namespace eShop.WebAppComponents.Protocol;

/// <summary>
/// 产品图片链接提供接口
/// </summary>
public interface IProductImageUrlProvider
{
    /// <summary>
    /// 获取产品图片链接
    /// </summary>
    /// <param name="catalogItem">目录</param>
    /// <returns>产品图片链接</returns>
    string GetProductImageUrl(CatalogItem catalogItem) => GetProductImageUrl(catalogItem.Id);
    
    /// <summary>
    /// 获取产品图片链接
    /// </summary>
    /// <param name="productId">产品id</param>
    /// <returns>产品图片链接</returns>
    string GetProductImageUrl(int productId);
}
