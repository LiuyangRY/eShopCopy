using eShop.Catalog.API.Models;

namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// 目录类型实体类型配置
/// </summary>
public class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
{
    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="builder">实体类型构建类</param>
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        builder.ToTable("CatalogType");
        builder.Property(catalogType => catalogType.Name)
            .HasMaxLength(100);
    }
}
