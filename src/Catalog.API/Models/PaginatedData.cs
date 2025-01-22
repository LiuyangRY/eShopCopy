namespace eShop.Catalog.API.Models;

/// <summary>
/// 分页数据
/// </summary>
/// <param name="pageIndex">页数索引</param>
/// <param name="pageSize">页面大小</param>
/// <param name="totalCount">总数据量</param>
/// <param name="data">数据</param>
/// <typeparam name="TEntity"></typeparam>
public class PaginatedData<TEntity>(int pageIndex, int pageSize, long totalCount, IEnumerable<TEntity> data) where TEntity : class
{
    /// <summary>
    /// 页数索引 
    /// </summary>
    public int PageIndex { get; } = pageIndex;
    
    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// 总数据量
    /// </summary>
    public long TotalCount { get; } = totalCount;

    /// <summary>
    /// 数据
    /// </summary>
    public IEnumerable<TEntity> Data { get; } = data;
}
