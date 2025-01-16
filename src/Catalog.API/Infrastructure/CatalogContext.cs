﻿using eShop.Catalog.API.Infrastructure.EntityConfigurations;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Infrastructure;

/// <summary>
/// 目录上下文
/// </summary>
public class CatalogContext : DbContext
{
    /// <summary>
    /// 目录上下文构造函数
    /// </summary>
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// 目录
    /// </summary>
    public DbSet<Model.Catalog> Catalogs { get; set; }
    
    /// <summary>
    /// 目录品牌
    /// </summary>
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    
    /// <summary>
    /// 目录类型
    /// </summary>
    public DbSet<CatalogType> CatalogTypes { get; set; }

    /// <summary>
    /// 创建数据
    /// </summary>
    /// <param name="builder">模型构建类</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("vector");
        builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogEntityConfiguration());
        // builder.
    }
}
