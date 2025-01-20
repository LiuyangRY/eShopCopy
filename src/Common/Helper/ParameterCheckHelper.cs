namespace Common.Helper;

/// <summary>
/// 参数校验帮助类
/// </summary>
public static class ParameterCheckHelper
{
    /// <summary>
    /// 参数不为空或空字符串校验
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="errorMsg">校验不通过错误消息</param>
    public static void IsNotNullOrWhitespace(this string parameter, string errorMsg)
    {
        if (string.IsNullOrWhiteSpace(parameter))
        {
            throw new ArgumentException(errorMsg);    
        }
    }

    /// <summary>
    /// 参数不为空校验
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="errorMsg">校验不通过错误信息</param>
    /// <typeparam name="T">参数类型</typeparam>
    public static void IsNotNull<T>(this T? parameter, string errorMsg) where T : class
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(errorMsg);
        }
    }
}
