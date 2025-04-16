using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eShop.ServiceDefaults;

/// <summary>
/// Http客户端扩展类
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// 添加授权令牌
    /// </summary>
    /// <param name="builder">Http客户端构建类</param>
    /// <returns>Http客户端构建类</returns>
    public static IHttpClientBuilder AddAuthToken(this IHttpClientBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddTransient<HttpClientAuthorizationDelegatingHandler>();
        builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
        return builder;
    }

    /// <summary>
    /// Http授权委托处理类
    /// </summary>
    private class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Http授权委托处理类构造函数
        /// </summary>
        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Http授权委托处理类构造函数
        /// </summary>
        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 发送Http异步请求
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Http响应</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext is HttpContext httpContext)
            {
                if (httpContext.Response.StatusCode is (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var accessToken = await httpContext.GetTokenAsync("access_token");
                    if (accessToken is not null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
