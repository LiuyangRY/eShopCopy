using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// 目录品牌实体类型配置
/// </summary>
public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="builder">实体类型构建类</param>
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable("CatalogBrand");
        builder.Property(catalogBrand => catalogBrand.Name)
            .HasMaxLength(100);
    }
}
