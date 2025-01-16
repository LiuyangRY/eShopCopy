using System.Diagnostics;

namespace eShop.Catalog.API.Extensions;

/// <summary>
/// 迁移数据库上下文扩展
/// </summary>
public static class MigrateDbContextExtensions
{
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    /// <summary>
    /// 迁移数据库上下文（不预置数据）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services) where TContext : DbContext
        => services.AddMigration<TContext>((_, _) => Task.CompletedTask);

    /// <summary>
    /// 迁移数据库上下文
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="seeder">数据种子委托</param>
    /// <typeparam name="TContext">数据库上下文</typeparam>
    /// <returns>服务集合</returns>
    private static IServiceCollection AddMigration<TContext>(this IServiceCollection services,
        Func<TContext, IServiceProvider, Task> seeder)
        where TContext : DbContext
    {
        // 启用迁移追踪
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(ActivitySourceName));
        return services.AddHostedService(serviceProvider =>
            new MigrationHostedService<TContext>(serviceProvider, seeder));
    }

    /// <summary>
    /// 迁移数据库上下文
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    /// <typeparam name="TDbSeeder">数据库种子类型</typeparam>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
        return services.AddMigration<TContext>((context, serviceProvider) =>
            serviceProvider.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
    }

    /// <summary>
    /// 迁移数据库上下文
    /// </summary>
    /// <param name="serviceProvider">服务提供类</param>
    /// <param name="seeder">数据库种子委托</param>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    private static async Task MigrateDbContextAsync<TContext>(this IServiceProvider serviceProvider,
        Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var scopedService = scope.ServiceProvider;
        var logger = scopedService.GetRequiredService<ILogger<TContext>>();
        var context = scopedService.GetRequiredService<TContext>();

        using var activity = ActivitySource.StartActivity($"迁移操作 {typeof(TContext).Name}");
        try
        {
            logger.LogInformation($"迁移数据库，上下文：{typeof(TContext).Name}");
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(() => InvokeSeeder(seeder, context, scopedService));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"迁移数据库时出现异常，数据库上下文：{typeof(TContext).Name}");
            activity.SetExceptionTags(exception);
            throw;
        }
    }

    /// <summary>
    /// 执行种子委托
    /// </summary>
    /// <param name="seeder">种子委托</param>
    /// <param name="context">数据库上下文</param>
    /// <param name="serviceProvider">服务提供类</param>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    private static async Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context,
        IServiceProvider serviceProvider) where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"迁移 {typeof(TContext).Name}");
        try
        {
            await context.Database.MigrateAsync();
            await seeder(context, serviceProvider);
        }
        catch (Exception exception)
        {
            activity.SetExceptionTags(exception);
            throw;
        }
    }

    /// <summary>
    /// 迁移宿主服务
    /// </summary>
    /// <param name="serviceProvider">服务提供类</param>
    /// <param name="seeder">种子委托</param>
    /// <typeparam name="TContext">数据库上下文</typeparam>
    private class MigrationHostedService<TContext>(
        IServiceProvider serviceProvider,
        Func<TContext, IServiceProvider, Task> seeder) : BackgroundService where TContext : DbContext
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }
    }
}

/// <summary>
/// 数据库种子接口
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IDbSeeder<in TContext> where TContext : DbContext
{
    /// <summary>
    /// 数据库种子委托
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>执行种子任务</returns>
    Task SeedAsync(TContext context);
}
