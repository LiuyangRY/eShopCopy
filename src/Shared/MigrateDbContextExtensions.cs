using System.Diagnostics;

namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// 迁移数据库上下文扩展
/// </summary>
internal static class MigrateDbContextExtensions
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
        if (context.Database.HasPendingModelChanges())
        {
            throw new Exception($"数据库上下文 {typeof(TContext).Name} 存在未提交的模型更改，可能会导致迁移失败。请先提交更改。");
        }
        using var activity = ActivitySource.StartActivity($"迁移操作 {typeof(TContext).Name}");
        try
        {
            logger.LogInformation($"迁移数据库，上下文：{typeof(TContext).Name}");
            var strategy = context.Database.CreateExecutionStrategy();
            var parameter = new SeederParameter<TContext>(seeder, context, scopedService);
            await strategy.ExecuteAsync(parameter, InvokeSeeder);
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
    /// <param name="parameter">种子委托</param>
    private static async Task InvokeSeeder<TContext>(SeederParameter<TContext> parameter) where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"迁移 {typeof(TContext).Name}");
        try
        {
            var logger = parameter.ServiceProvider.GetRequiredService<ILogger<TContext>>();
            logger.LogInformation("开始执行数据库迁移...");
            await parameter.Context.Database.MigrateAsync();
            logger.LogInformation("数据库迁移完成");
            await parameter.Seeder(parameter.Context, parameter.ServiceProvider);
        }
        catch (Exception exception)
        {
            activity.SetExceptionTags(exception);
            throw;
        }
    }

    /// <summary>
    /// 种子委托参数
    /// </summary>
    /// <param name="Seeder">种子委托</param>
    /// <param name="Context">数据库上下文</param>
    /// <param name="ServiceProvider">服务提供类</param>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    private record SeederParameter<TContext>(
        Func<TContext, IServiceProvider, Task> Seeder,
        TContext Context,
        IServiceProvider ServiceProvider) where TContext : DbContext;

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