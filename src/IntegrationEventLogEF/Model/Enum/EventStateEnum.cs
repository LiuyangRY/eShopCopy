namespace eShop.IntegrationEventLogEF.Model.Enum;

/// <summary>
/// 事件状态枚举
/// </summary>
public enum EventStateEnum
{
    /// <summary>
    /// 未发布
    /// </summary>
    NotPublished = 0,
        
    /// <summary>
    /// 发布中
    /// </summary>
    InPublishProgress = 1,
    
    /// <summary>
    /// 已发布
    /// </summary>
    Published = 2,
    
    /// <summary>
    /// 发布失败
    /// </summary>
    PublishedFailed = 3,
}
