using System.ComponentModel;

namespace Identity.API.Models.Account;

/// <summary>
/// 认证视图模型
/// </summary>
/// <param name="applicationName">应用名称</param>
/// <param name="scope">作用域</param> 
public class AuthorizeViewModel(string? applicationName, string? scope)
{
    /// <summary>
    /// 应用名称
    /// </summary>
    [DisplayName("应用名称")] 
    public string? ApplicationName { get; set; } = applicationName;

    /// <summary>
    /// 作用域
    /// </summary>
    [DisplayName("作用域")] 
    public string? Scope { get; set; } = scope;
}
