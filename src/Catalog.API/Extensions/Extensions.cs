using eShop.IntegrationEventLogEF.Protocols;
using eShop.IntegrationEventLogEF.Services;

namespace eShop.Catalog.API.Extensions;

/// <summary>
/// 扩展类
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 添加应用程序服务
    /// </summary>
    public static void AddApplicationServices(this IHostApplicationBuilder hostBuilderer)
    {
        if (hostBuilderer.Environment.IsBuild())
        {
            // 如果项目是 OpenApi 文档生成启动的，避免加载所有数据库配置和迁移
            hostBuilderer.Services.AddDbContext<CatalogContext>();
            return;
        }
        hostBuilderer.AddNpgsqlDbContext<CatalogContext>("catalogDb", configureDbContextOptions: dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(hostBuilderer.Configuration.GetConnectionString("catalogDb"), optionsBuilder =>
            {
                optionsBuilder.UseVector();
            });
        });
        if (hostBuilderer.Environment.IsDevelopment())
        {
            hostBuilderer.Services.AddMigration<CatalogContext, CatalogContextSeed>();
        }

        hostBuilderer.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();
        hostBuilderer.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));
    }
}
