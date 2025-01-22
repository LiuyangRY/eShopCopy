namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// 目录实体配置
/// </summary>
public class CatalogEntityConfiguration : IEntityTypeConfiguration<Models.Catalog>
{
    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="builder">实体类型构建类</param>
    public void Configure(EntityTypeBuilder<Models.Catalog> builder)
    {
        builder.ToTable("Catalog");
        builder.Property(catalog => catalog.Name)
            .HasMaxLength(50);
        builder.Property(catalog => catalog.Embedding)
            .HasColumnType("vector(384)");
        builder.HasOne(catalog => catalog.CatalogBrand)
            .WithMany();
        builder.HasOne(catalog => catalog.CatalogType)
            .WithMany();
        builder.HasIndex(catalog => catalog.Name);
    }
}
