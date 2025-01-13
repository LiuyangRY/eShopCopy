using System.Net.Http.Json;
using WebAppComponents.Catalog;
using WebAppComponents.Protocol;

namespace WebAppComponents.Services;

/// <summary>
/// 目录服务
/// </summary>
/// <param name="httpClient">http客户端</param>
public class CatalogService(HttpClient httpClient) : ICatalogService
{
    /// <summary>
    /// 远程服务基本url
    /// </summary>
    private const string RemoteServiceBaseUrl = "api/catalog/";

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
        var uri = $"{RemoteServiceBaseUrl}catalogBrands";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CatalogBrand>>(uri);
        return result!;
    }

    /// <summary>
    /// 获取目录类型
    /// </summary>
    /// <returns>目录类型</returns>
    public async Task<IEnumerable<CatalogType>> GetCatalogTypesAsync()
    {
        var uri = $"{RemoteServiceBaseUrl}catalogTypes";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<CatalogType>>(uri);
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
            var brandQs = brandId.HasValue ? brandId.Value.ToString() : string.Empty;
            filterQs = $"/type/{typeId.Value}/brand/{brandQs}";
        }
        else if (brandId.HasValue)
        {
            filterQs = $"/type/all/brand/{brandId.Value}";
        }

        return $"{RemoteServiceBaseUrl}items{filterQs}?pageIndex={pageIndex}&pageSize={pageSize}";
    }
}
