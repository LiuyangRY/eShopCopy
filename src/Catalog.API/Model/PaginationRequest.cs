using System.ComponentModel;

namespace eShop.Catalog.API.Model;

/// <summary>
/// 分页请求参数
/// </summary>
/// <param name="PageSize">单页返回的数据量</param>
/// <param name="PageIndex">查询分页数据的页码索引</param>
public record PaginationRequest(
    [property: Description("单页返回的数据量")]
    [property: DefaultValue(10)]
    int PageSize = 10,
    
    [property: Description("查询分页数据的页码索引")]
    [property: DefaultValue(0)]
    int PageIndex = 0
);
