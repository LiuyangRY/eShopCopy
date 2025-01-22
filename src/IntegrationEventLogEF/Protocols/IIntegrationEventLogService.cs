namespace eShop.IntegrationEventLogEF.Protocols;

/// <summary>
/// 集成事件日志服务接口
/// </summary>
public interface IIntegrationEventLogService
{
    /// <summary>
    /// 获取待发布的事件日志
    /// </summary>
    /// <param name="transactionId">事务id</param>
    /// <returns>待发布的事件日志列表</returns>
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);

    /// <summary>
    /// 保存事件
    /// </summary>
    /// <param name="event">事件</param>
    /// <param name="transaction">数据库事务</param>
    /// <returns>保存事件任务</returns>
    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);

    /// <summary>
    /// 将事件标记为已发布
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记已发布事件任务</returns>
    Task MarkEventAsPublishedAsync(Guid eventId);

    /// <summary>
    /// 将事件标记为发布中
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记发布中事件任务</returns>
    Task MarkEventAsInPublishProgressAsync(Guid eventId);

    /// <summary>
    /// 将事务标记为发布失败
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记发布失败事件任务</returns>
    Task MarkEventAsPublishedFailedAsync(Guid eventId);
}
