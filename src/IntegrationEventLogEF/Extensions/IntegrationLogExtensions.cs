using eShop.IntegrationEventLogEF.Model;

namespace eShop.IntegrationEventLogEF.Extensions;

/// <summary>
/// 集成日志扩展
/// </summary>
public static class IntegrationLogExtensions
{
    /// <summary>
    /// 使用集成事件日志
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static void UseIntegrationEventLogs(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntegrationEventLogEntry>(builder =>
        {
            builder.ToTable("IntegrationEventLog");
            builder.HasKey(e => e.EventId);
        });
    }
}
