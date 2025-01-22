using eShop.Catalog.API.Models;

namespace eShop.Catalog.API.Apis;

/// <summary>
/// 目录类型Api
/// </summary>
public static class TypeApi
{
    /// <summary>
    /// 映射目录ApiV1
    /// </summary>
    /// <param name="builder">终结点路由构建类</param>
    /// <returns>终结点路由构建类</returns>
    public static IEndpointRouteBuilder MapTypeApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/type").HasApiVersion(1.0);
        api.MapGet("/all", GetAllTypesAsync)
            .WithName("目录类型数据")
            .WithSummary("所有目录类型数据")
            .WithDescription("获取所有目录类型数据列表")
            .WithTags("目录类型");
        return builder;
    }

    /// <summary>
    /// 获取所有目录品牌
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>所有目录品牌数据</returns>
    private static async Task<Ok<List<CatalogType>>> GetAllTypesAsync(CatalogContext context)
    {
        var data = await context.CatalogTypes.OrderBy(brand => brand.Name).ToListAsync(); 
        return TypedResults.Ok(data);
    }
}
