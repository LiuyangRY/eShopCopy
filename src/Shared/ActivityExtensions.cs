using System.Diagnostics;

/// <summary>
/// 活动扩展
/// </summary>
internal static class ActivityExtensions
{
    /// <summary>
    /// 设置异常标签
    /// </summary>
    /// <param name="activity">活动</param>
    /// <param name="exception">异常</param>
    public static void SetExceptionTags(this Activity? activity, Exception exception)
    {
        if (activity is null)
        {
            return;
        }
        activity.AddTag("exception.message", exception.Message);
        activity.AddTag("exception.stacktrace", exception.ToString());
        activity.AddTag("exception.type", exception.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
