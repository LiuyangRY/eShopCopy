using eShop.WebAppComponents.Protocols;

namespace eShop.WebApp.Services;

/// <summary>
/// 产品图片链接提供类
/// </summary>
public class ProductImageUrlProvider : IProductImageUrlProvider
{
    /// <summary>
    /// 获取产品图片链接
    /// </summary>
    /// <param name="productId">产品id</param>
    /// <returns>产品图片链接</returns>
    public string GetProductImageUrl(int productId) => $"product-images/{productId}?api-version={ServiceConstant.CatalogApiVersion}";
}
