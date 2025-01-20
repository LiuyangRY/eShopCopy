using System.ComponentModel;
using eShop.Catalog.API.Helper.Builder;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Apis;

/// <summary>
/// 目录Api
/// </summary>
public static class CatalogApi
{
    /// <summary>
    /// 目录Api标签名称
    /// </summary>
    private const string CatalogApiTagName = "目录";

    /// <summary>
    /// 映射目录ApiV1
    /// </summary>
    /// <param name="builder">终结点路由构建类</param>
    /// <returns>终结点路由构建类</returns>
    public static IEndpointRouteBuilder MapCatalogApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/catalog").HasApiVersion(1.0);
        api.MapGet("/{id:int}", GetCatalogByIdAsync)
            .WithName("根据目录id获取目录数据")
            .WithSummary("根据目录id获取目录数据")
            .WithDescription("根据目录id获取目录数据")
            .WithTags(CatalogApiTagName);
        api.MapGet("/paging", GetPaginatedCatalogAsync)
            .WithName("获取目录分页数据")
            .WithSummary("目录分页数据列表")
            .WithDescription("目录分页数据列表")
            .WithTags(CatalogApiTagName);
        api.MapGet("/paging/type/{typeId:int}", GetPaginatedCatalogByTypeIdAsync)
            .WithName("根据类型id获取目录分页数据")
            .WithSummary("根据类型id获取目录分页数据列表")
            .WithDescription("根据类型id获取目录分页数据列表")
            .WithTags(CatalogApiTagName);
        api.MapGet("/paging/brand/{brandId:int}", GetPaginatedCatalogByBrandIdAsync)
            .WithName("根据品牌id获取目录分页数据")
            .WithSummary("根据品牌id获取目录分页数据列表")
            .WithDescription("根据品牌id获取目录分页数据列表")
            .WithTags(CatalogApiTagName);
        api.MapGet("/paging/type/{typeId:int}/brand/{brandId:int}", GetPaginatedCatalogByTypeIdAndBrandIdAsync)
            .WithName("根据类型id和品牌id获取目录分页数据")
            .WithSummary("根据类型id和品牌id获取目录分页数据列表")
            .WithDescription("根据类型id和品牌id获取目录分页数据列表")
            .WithTags(CatalogApiTagName);
        api.MapGet("/{id:int}/pic", GetCatalogPictureByIdAsync)
            .WithName("获取目录图片")
            .WithSummary("根据目录id获取目录图片")
            .WithDescription("根据目录id获取目录图片")
            .WithTags(CatalogApiTagName);
        return builder;
    }

    /// <summary>
    /// 根据目录id获取目录数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="id">目录id</param>
    /// <returns>目录数据</returns>
    private static async Task<Results<Ok<Model.Catalog>, NotFound, BadRequest<ProblemDetails>>> GetCatalogByIdAsync(CatalogContext context,
        [Description("目录id")] int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest<ProblemDetails>(new() { Detail = "目录id无效" });
        }

        var result = await context.Catalogs.Include(catalog => catalog.CatalogBrand)
            .Include(catalog => catalog.CatalogType)
            .FirstOrDefaultAsync(catalog => catalog.Id == id);
        if (result is null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// 获取目录分页数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogAsync(CatalogContext context,
        [AsParameters] PaginationRequest paginationRequest)
    {
        return await GetCatalogByConditionAsync(context, paginationRequest);
    }

    /// <summary>
    /// 根据类型id获取目录分页数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="typeId">类型id</param>
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByTypeIdAsync(CatalogContext context,
        [AsParameters] PaginationRequest paginationRequest, int typeId)
    {
        return await GetCatalogByConditionAsync(context, paginationRequest, typeId: typeId);
    }

    /// <summary>
    /// 根据品牌id获取目录分页数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="brandId">品牌id</param>
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByBrandIdAsync(
        CatalogContext context,
        [AsParameters] PaginationRequest paginationRequest, int brandId)
    {
        return await GetCatalogByConditionAsync(context, paginationRequest, brandId: brandId);
    }

    /// <summary>
    /// 根据类型id和品牌id获取目录分页数据 
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="typeId">类型id</param>
    /// <param name="brandId">品牌id</param>
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByTypeIdAndBrandIdAsync(
        CatalogContext context,
        [AsParameters] PaginationRequest paginationRequest, int typeId, int brandId)
    {
        return await GetCatalogByConditionAsync(context, paginationRequest, typeId: typeId, brandId: brandId);
    }

    /// <summary>
    /// 根据目录id获取目录图片
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="environment">Web宿主环境</param>
    /// <param name="id">目录id</param>
    /// <returns>目录图片</returns>
    private static async Task<Results<PhysicalFileHttpResult, NotFound>> GetCatalogPictureByIdAsync(
        CatalogContext context,
        IWebHostEnvironment environment, [Description("目录id")] int id)
    {
        var catalog = await context.Catalogs.FindAsync(id);
        if (catalog is null)
        {
            return TypedResults.NotFound();
        }

        var path = Path.Combine(environment.ContentRootPath, "pics", catalog.PictureFileName);
        var imageFileExtension = Path.GetExtension(catalog.PictureFileName);
        var mimeType = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        var lastModified = File.GetLastWriteTimeUtc(path);
        return TypedResults.PhysicalFile(path, mimeType, lastModified: lastModified);
    }

    /// <summary>
    /// 根据条件获取目录数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="typeId">类型id</param>
    /// <param name="brandId">品牌id</param> 
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetCatalogByConditionAsync(CatalogContext context,
        PaginationRequest paginationRequest, int? typeId = null, int? brandId = null)
    {
        var conditionExpression = new CatalogFilterBuilder()
            .WithTypeId(typeId)
            .WithBrandId(brandId)
            .Build();
        var totalCount = await context.Catalogs.Where(conditionExpression)
            .LongCountAsync();
        var catalogs = await context.Catalogs
            .Where(conditionExpression)
            .OrderBy(catalog => catalog.Name)
            .Skip(paginationRequest.PageSize * paginationRequest.PageIndex)
            .Take(paginationRequest.PageSize)
            .ToListAsync();
        return TypedResults.Ok(new PaginatedData<Model.Catalog>(paginationRequest.PageIndex, paginationRequest.PageSize,
            totalCount, catalogs));
    }

    /// <summary>
    /// 根据图片文件扩展名称获取MIME类型
    /// </summary>
    /// <param name="extension">图片文件扩展名称</param>
    /// <returns>MIME类型</returns>
    private static string GetImageMimeTypeFromImageFileExtension(string extension) => extension switch
    {
        ".png" => "image/png",
        ".jpg" => "image/jpeg",
        ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".tiff" => "image/tiff",
        ".bmp" => "image/bmp",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };
}
