﻿using eShop.Catalog.API.Helper.Builder;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Apis;

/// <summary>
/// 目录Api
/// </summary>
public static class CatalogApi
{
    /// <summary>
    /// 映射目录ApiV1
    /// </summary>
    /// <param name="builder">终结点路由构建类</param>
    /// <returns>终结点路由构建类</returns>
    public static IEndpointRouteBuilder MapCatalogApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/catalog").HasApiVersion(1.0);
        api.MapGet("/paging", GetPaginatedCatalog)
            .WithName("获取目录分页数据")
            .WithSummary("目录分页数据列表")
            .WithDescription("目录分页数据列表")
            .WithTags("目录");
        api.MapGet("/paging/type/{typeId:int}", GetPaginatedCatalogByTypeId)
            .WithName("根据类型id获取目录分页数据")
            .WithSummary("根据类型id获取目录分页数据列表")
            .WithDescription("根据类型id获取目录分页数据列表")
            .WithTags("目录");
        api.MapGet("/paging/brand/{brandId:int}", GetPaginatedCatalogByBrandId)
            .WithName("根据品牌id获取目录分页数据")
            .WithSummary("根据品牌id获取目录分页数据列表")
            .WithDescription("根据品牌id获取目录分页数据列表")
            .WithTags("目录");
        api.MapGet("/paging/type/{typeId:int}/brand/{brandId:int}", GetPaginatedCatalogByTypeIdAndBrandId)
            .WithName("根据类型id和品牌id获取目录分页数据")
            .WithSummary("根据类型id和品牌id获取目录分页数据列表")
            .WithDescription("根据类型id和品牌id获取目录分页数据列表")
            .WithTags("目录");
        return builder;
    }

    /// <summary>
    /// 获取目录分页数据
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <returns>分页目录数据</returns>
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalog(CatalogContext context,
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
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByTypeId(CatalogContext context,
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
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByBrandId(CatalogContext context,
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
    private static async Task<Ok<PaginatedData<Model.Catalog>>> GetPaginatedCatalogByTypeIdAndBrandId(
        CatalogContext context,
        [AsParameters] PaginationRequest paginationRequest, int typeId, int brandId)
    {
        return await GetCatalogByConditionAsync(context, paginationRequest, typeId: typeId, brandId: brandId);
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
}
