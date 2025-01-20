using System.Reflection;

namespace eShop.IntegrationEventLogEF.Services;

/// <summary>
/// 集成事件日志服务
/// </summary>
public class IntegrationEventLogService<TContext> : IIntegrationEventLogService where TContext : DbContext
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    private readonly TContext _context;

    /// <summary>
    /// 事件类型
    /// </summary>
    private readonly Type[] _eventTypes;

    /// <summary>
    /// 集成事件日志服务构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public IntegrationEventLogService(TContext context)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        entryAssembly.IsNotNull("未找到应用程序入口程序集");
        _context = context;
        _eventTypes = Assembly.Load(entryAssembly!.FullName!)
            .GetTypes()
            .Where(type => type.Name.EndsWith(nameof(IntegrationEvent)))
            .ToArray();
    }

    /// <summary>
    /// 获取待发布的事件日志
    /// </summary>
    /// <param name="transactionId">事务id</param>
    /// <returns>待发布的事件日志列表</returns>
    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var result = await _context.Set<IntegrationEventLogEntry>()
            .Where(e => e.TransactionId == transactionId && e.State == EventStateEnum.NotPublished)
            .ToListAsync();
        if (result.Count > 0)
        {
            return result.OrderBy(e => e.CreatedTime)
                .Select(e =>
                {
                    var type = _eventTypes.FirstOrDefault(type => type.Name == e.EventTypeShortName);
                    type.IsNotNull($"未发现指定事件类型{e.EventTypeShortName}");
                    return e.DeserializeJsonContent(type!);
                })
                .ToList();
        }

        return [];
    }

    /// <summary>
    /// 保存事件
    /// </summary>
    /// <param name="event">事件</param>
    /// <param name="transaction">数据库事务</param>
    /// <returns>保存事件任务</returns>
    public async Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        transaction.IsNotNull("保存事件时数据库事务不允许为空");
        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);
        await _context.Database.UseTransactionAsync(transaction.GetDbTransaction());
        _context.Set<IntegrationEventLogEntry>().Add(eventLogEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 将事件标记为已发布
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记已发布事件任务</returns>
    public async Task MarkEventAsPublishedAsync(Guid eventId)
    {
        await UpdateEventStateAsync(eventId, EventStateEnum.Published);
    }

    /// <summary>
    /// 将事件标记为发布中
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记发布中事件任务</returns>
    public async Task MarkEventAsInPublishProgressAsync(Guid eventId)
    {
        await UpdateEventStateAsync(eventId, EventStateEnum.InPublishProgress);
    }

    /// <summary>
    /// 将事务标记为发布失败
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <returns>标记发布失败事件任务</returns> 
    public async Task MarkEventAsPublishedFailedAsync(Guid eventId)
    {
        await UpdateEventStateAsync(eventId, EventStateEnum.PublishedFailed);
    }

    /// <summary>
    /// 更新事件状态
    /// </summary>
    /// <param name="eventId">事件id</param>
    /// <param name="state">事件状态</param>
    /// <returns>更新事件状态任务</returns>
    private async Task UpdateEventStateAsync(Guid eventId, EventStateEnum state)
    {
        var eventLogEntry = _context.Set<IntegrationEventLogEntry>()
            .FirstOrDefault(eventLogEntry => eventLogEntry.EventId == eventId);
        eventLogEntry.IsNotNull($"未查询到事件日志实体，事件id:{eventId}");
        eventLogEntry!.State = state;
        if (state == EventStateEnum.InPublishProgress)
        {
            eventLogEntry.SentTimes++;
        }

        await _context.SaveChangesAsync();
    }
}
