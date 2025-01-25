using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identity.API.Extensions;

/// <summary>
/// 安全头属性
/// </summary>
public class SecurityHeadersAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 执行前操作
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ViewResult)
        {
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options 
            AddResponseHeader(context, "X-Content-Type-Options", "nosniff");
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options 
            AddResponseHeader(context, "X-Frame-Options", "SAMEORIGIN");
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy 
            AddResponseHeader(context, "Referrer-Policy", "no-referrer");
        }
    }

    /// <summary>
    /// 添加响应头
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    /// <param name="headerName"></param>
    /// <param name="value"></param>
    private void AddResponseHeader(ResultExecutingContext context, string headerName, string value)
    {
        if (!context.HttpContext.Response.Headers.ContainsKey(headerName))
        {
            context.HttpContext.Response.Headers.Append(headerName, value);
        }
    }
}
