using System.Net.Http.Json;
using eShop.WebAppComponents.Model;
using eShop.WebAppComponents.Protocol;

namespace eShop.WebAppComponents.Services;

/// <summary>
/// 目录服务
/// </summary>
/// <param name="httpClient">http客户端</param>
public class CatalogService(HttpClient httpClient) : ICatalogService
{
    /// <summary>
    /// 根据目录id获取目录数据
    /// </summary>
    /// <param name="catalogId">目录id</param>
    /// <returns>目录数据</returns>
    public async Task<CatalogItem?> GetCatalogByIdAsync(int catalogId)
    {
        return await httpClient.GetFromJsonAsync<CatalogItem>($"/api/catalog/{catalogId}");
    }
    
    /// <summary>
    /// 获取目录数据 
    /// </summary>
    /// <param name="pageIndex">页数</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="brandId">品牌id</param>
    /// <param name="typeId">类型id</param>
    /// <returns>目录数据 </returns>
    public async Task<CatalogResult?> GetCatalogItemsAsync(int pageIndex, int pageSize, int? brandId, int? typeId)
    {
        var uri = GetCatalogItemsUri(pageIndex, pageSize, brandId, typeId);
        var result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);
        return result;
    }

    /// <summary>
    /// 获取目录品牌
    /// </summary>
    /// <returns>目录品牌</returns>
    public async Task<IEnumerable<CatalogBrand>> GetCatalogBrandsAsync()
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CatalogBrand>>("/api/brand/all");
        return result!;
    }

    /// <summary>
    /// 获取目录类型
    /// </summary>
    /// <returns>目录类型</returns>
    public async Task<IEnumerable<CatalogType>> GetCatalogTypesAsync()
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CatalogType>>("/api/type/all");
        return result!;
    }

    /// <summary>
    /// 获取目录请求地址
    /// </summary>
    /// <param name="pageIndex">页数</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="brandId">品牌id</param>
    /// <param name="typeId">类型id</param>
    /// <returns>目录请求地址</returns>
    private static string GetCatalogItemsUri(int pageIndex, int pageSize, int? brandId, int? typeId)
    {
        string filterQs = string.Empty;
        if (typeId.HasValue)
        {
            filterQs = $"/type/{typeId.Value}";
        }
        if (brandId.HasValue)
        {
            filterQs += $"/brand/{brandId.Value}";
        }

        return $"api/catalog/paging{filterQs}?pageIndex={pageIndex}&pageSize={pageSize}";
    }
}
