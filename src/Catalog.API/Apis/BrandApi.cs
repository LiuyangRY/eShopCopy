using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Apis;

/// <summary>
/// 目录品牌Api
/// </summary>
public static class BrandApi
{
    /// <summary>
    /// 映射目录品牌ApiV1
    /// </summary>
    /// <param name="builder">终结点路由构建类</param>
    /// <returns>终结点路由构建类</returns>
    public static IEndpointRouteBuilder MapBrandApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/brand").HasApiVersion(1.0);
        api.MapGet("/all", GetAllBrandsAsync)
            .WithName("目录品牌数据")
            .WithSummary("所有目录品牌数据")
            .WithDescription("获取所有目录品牌数据列表")
            .WithTags("目录品牌");
        return builder;
    }

    /// <summary>
    /// 获取所有目录品牌
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>所有目录品牌数据</returns>
    private static async Task<Ok<List<CatalogBrand>>> GetAllBrandsAsync(CatalogContext context)
    {
        var data = await context.CatalogBrands.OrderBy(brand => brand.Name).ToListAsync(); 
        return TypedResults.Ok(data);
    }
}
