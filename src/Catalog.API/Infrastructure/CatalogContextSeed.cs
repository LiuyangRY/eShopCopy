using System.Text.Json;
using eShop.Catalog.API.Models;
using Npgsql;

namespace eShop.Catalog.API.Infrastructure;

/// <summary>
/// 目录上下文种子
/// </summary>
/// <param name="webHostEnvironment">Web宿主环境</param>
/// <param name="logger">日志</param>
public class CatalogContextSeed(IWebHostEnvironment webHostEnvironment, ILogger<CatalogContextSeed> logger)
    : IDbSeeder<CatalogContext>
{
    /// <summary>
    /// 数据库种子委托
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>执行种子任务</returns>
    public async Task SeedAsync(CatalogContext context)
    {
        var contentRootPath = webHostEnvironment.ContentRootPath;
        await context.Database.OpenConnectionAsync();
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.ReloadTypesAsync();
        if (!context.Catalogs.Any())
        {
            var sourcePath = Path.Combine(contentRootPath, "Setup", "catalog.json");
            var sourceJson = await File.ReadAllTextAsync(sourcePath);
            var sourceItems = JsonSerializer.Deserialize<CatalogSourceEntry[]>(sourceJson);
            if (sourceItems == null)
                return;
            var brandNameSet = new HashSet<string>(sourceItems.Length);
            var typeNameSet = new HashSet<string>(sourceItems.Length);
            var catalogIdSet = new HashSet<int>(sourceItems.Length);
            var catalogBrands = new List<CatalogBrand>(sourceItems.Length);
            var catalogTypes = new List<CatalogType>(sourceItems.Length);
            var catalogs = new List<Models.Catalog>(sourceItems.Length);
            var now = DateTime.Now.ToUniversalTime();
            foreach (var item in sourceItems)
            {
                CatalogBrand? catalogBrand = null;
                CatalogType? catalogType = null;
                if (!string.IsNullOrWhiteSpace(item.Brand) && brandNameSet.Add(item.Brand))
                {
                    catalogBrand = new CatalogBrand
                    {
                        Name = item.Brand,
                        Id = 0,
                        CreatedBy = 0,
                        CreatedAt = now,
                        UpdatedBy = 0,
                        UpdatedAt = now
                    };
                    catalogBrands.Add(catalogBrand);
                }

                if (!string.IsNullOrWhiteSpace(item.Type) && typeNameSet.Add(item.Type))
                {
                    catalogType = new CatalogType
                    {
                        Name = item.Type,
                        Id = 0,
                        CreatedBy = 0,
                        CreatedAt = now,
                        UpdatedBy = 0,
                        UpdatedAt = now
                    };
                    catalogTypes.Add(catalogType);
                }

                if (catalogBrand != null && catalogType != null && catalogIdSet.Add(item.Id))
                {
                    catalogs.Add(new Models.Catalog
                    {
                        Id = item.Id,
                        Name = item.Name ?? string.Empty,
                        Description = item.Description,
                        Price = item.Price,
                        CatalogBrand = catalogBrand,
                        CatalogType = catalogType,
                        AvailableStock = 100,
                        MaxStockThreshold = 200,
                        RestockThreshold = 10,
                        PictureFileName = $"{item.Id}.webp",
                        CreatedBy = 0,
                        CreatedAt = now,
                        UpdatedBy = 0,
                        UpdatedAt = now
                    });
                }
            }

            context.CatalogBrands.RemoveRange(context.CatalogBrands);
            await context.CatalogBrands.AddRangeAsync(catalogBrands);
            logger.LogInformation($"添加了{context.CatalogBrands.Count()}条目录品牌种子数据。");
            context.CatalogTypes.RemoveRange(context.CatalogTypes);
            await context.CatalogTypes.AddRangeAsync(catalogTypes);
            logger.LogInformation($"添加了{context.CatalogTypes.Count()}条目录类型种子数据。");
            await context.SaveChangesAsync();

            var brandIdsByNameDic = await context.CatalogBrands.ToDictionaryAsync(catalogBrand => catalogBrand.Name,
                catalogBrand => catalogBrand.Id);
            var typeIdsByNameDic =
                await context.CatalogTypes.ToDictionaryAsync(catalogType => catalogType.Name,
                    catalogType => catalogType.Id);
            foreach (var catalog in catalogs)
            {
                if (brandIdsByNameDic.TryGetValue(catalog.CatalogBrand.Name, out var brandId))
                {
                    catalog.CatalogBrandId = brandId;
                }

                if (typeIdsByNameDic.TryGetValue(catalog.CatalogType.Name, out var typeId))
                {
                    catalog.CatalogTypeId = typeId;
                }
            }

            await context.Catalogs.AddRangeAsync(catalogs);
            logger.LogInformation($"添加了{context.Catalogs.Count()}条目录种子数据。");
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 目录种子实体结构
    /// </summary>
    private class CatalogSourceEntry
    {
        public int Id { get; set; }

        public string? Type { get; set; }

        public string? Brand { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }
    }
}
