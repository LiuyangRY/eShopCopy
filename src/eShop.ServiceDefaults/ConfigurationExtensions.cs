using Microsoft.Extensions.Configuration;

namespace eShop.ServiceDefaults;

/// <summary>
/// 配置扩展
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <param name="configuration">配置</param>
    /// <param name="name">配置项</param>
    /// <returns>配置值</returns>
    public static string GetRequiredValue(this IConfiguration configuration, string name) => configuration[name] ??
        throw new InvalidOperationException(
            $"配置缺少值：{(configuration is IConfigurationSection section ? $"{section.Path}:{name}" : name)}");
}
