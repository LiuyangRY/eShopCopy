namespace eShop.Catalog.API.Model;

/// <summary>
/// 目录服务
/// </summary>
/// <param name="context">目录数据库上下文</param>
/// <param name="options">目录配置</param>
/// <param name="logger">目录日志</param>
public class CatalogServices(CatalogContext context, IOptions<CatalogOptions> options, ILogger<CatalogServices> logger)
{
    /// <summary>
    /// 目录数据库上下文
    /// </summary>
    public CatalogContext Context { get; } = context;

    /// <summary>
    /// 目录配置
    /// </summary>
    public IOptions<CatalogOptions> Options { get; } = options;
    
    /// <summary>
    /// 目录日志
    /// </summary>
    public ILogger<CatalogServices> Logger { get; } = logger;
}
