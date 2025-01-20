namespace eShop.IntegrationEventLogEF.Model;

/// <summary>
/// 集成事件日志实体
/// </summary>
public class IntegrationEventLogEntry
{
    /// <summary>
    /// 缩进选项
    /// </summary>
    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

    /// <summary>
    /// 大小写敏感选项
    /// </summary>
    public static readonly JsonSerializerOptions CaseSensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 集成事件日志实体无参构造函数（初始化数据库时使用）
    /// </summary>
    public IntegrationEventLogEntry()
    {
        EventTypeName = string.Empty;
        Content = string.Empty;
    }

    /// <summary>
    /// 集成事件日志实体构造函数
    /// </summary>
    /// <param name="event">集成事件</param>
    /// <param name="transactionId">事务id</param>
    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
    {
        @event.GetType().FullName!.IsNotNullOrWhitespace("事件类型名称不能为空");
        EventId = @event.Id;
        CreatedTime = @event.CreatedTime;
        EventTypeName = @event.GetType().FullName!;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), IndentedOptions);
        State = EventStateEnum.NotPublished;
        SentTimes = 0;
        TransactionId = transactionId;
    }

    /// <summary>
    /// 事件id
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// 事件类型名称
    /// </summary>
    [Required]
    public string EventTypeName { get; set; }

    /// <summary>
    /// 事件类型短名
    /// </summary>
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.').Last();

    /// <summary>
    /// 集成事件
    /// </summary>
    [NotMapped]
    public IntegrationEvent? IntegrationEvent { get; private set; }

    /// <summary>
    /// 事件状态
    /// </summary>
    public EventStateEnum State { get; set; }

    /// <summary>
    /// 发送次数
    /// </summary>
    public int SentTimes { get; set; }

    /// <summary>
    ///  创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// 事务id
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 反序列化Json内容
    /// </summary>
    /// <param name="type">反序列化目标类型</param>
    /// <returns>集成事件日志实体</returns>
    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        IntegrationEvent = JsonSerializer.Deserialize(Content, type, CaseSensitiveOptions) as IntegrationEvent;
        return this;
    }
}
