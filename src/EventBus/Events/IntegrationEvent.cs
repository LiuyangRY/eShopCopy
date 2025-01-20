using System.Text.Json.Serialization;

namespace eShop.EventBus.Events;

/// <summary>
/// 集成事件
/// </summary>
public class IntegrationEvent
{
    /// <summary>
    /// 事件id
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}
