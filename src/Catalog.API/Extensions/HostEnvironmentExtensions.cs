using System.Reflection;

namespace eShop.Catalog.API.Extensions;

/// <summary>
/// 宿主环境扩展类
/// </summary>
internal static class HostEnvironmentExtensions
{
    /// <summary>
    /// 是否构建环境
    /// </summary>
    /// <param name="hostEnvironment">宿主环境</param>
    /// <returns>是构建环境返回true，否则返回false</returns>
    public static bool IsBuild(this IHostEnvironment hostEnvironment)
    {
        // 当前是构建环境或由 OpenAPI 文档生成的工具 GetDocument.Insider 启动
        return hostEnvironment.IsEnvironment("Build") || Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
