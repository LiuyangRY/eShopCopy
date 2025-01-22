namespace eShop.WebAppComponents.Models;

/// <summary>
/// 目录结果
/// </summary>
/// <param name="PageIndex">页数</param>
/// <param name="PageSize">页面大小</param>
/// <param name="Count">目录总数</param>
/// <param name="Data">目录数据</param>

public record CatalogResult(int PageIndex, int PageSize, int Count, List<CatalogItem> Data);
