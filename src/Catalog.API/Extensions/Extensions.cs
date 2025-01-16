using eShop.Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eShop.Catalog.API.Extensions;

/// <summary>
/// 扩展类
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 添加应用程序服务
    /// </summary>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsBuild())
        {
            // 如果项目是 OpenApi 文档生成启动的，避免加载所有数据库配置和迁移
            builder.Services.AddDbContext<CatalogContext>();
            return;
        }
        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", configureDbContextOptions: dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(optionsBuilder =>
            {
                optionsBuilder.UseVector();
            });
        });
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();
        }
        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));
    }
}
