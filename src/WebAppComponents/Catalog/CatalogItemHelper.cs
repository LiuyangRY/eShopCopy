namespace eShop.WebAppComponents.Catalog;

/// <summary>
/// 目录项帮助类
/// </summary>
public static class CatalogItemHelper
{
    /// <summary>
    /// 获取目录项链接
    /// </summary>
    /// <param name="catalogItem">目录项</param>
    /// <returns>目录项链接</returns>
    public static string Url(CatalogItem catalogItem) => $"item/{catalogItem.Id}";
}
