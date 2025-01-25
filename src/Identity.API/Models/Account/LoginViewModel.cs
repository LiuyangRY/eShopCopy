namespace Identity.API.Models.Account;

/// <summary>
/// 登录视图模型
/// </summary>
public class LoginViewModel : LoginInputModel
{
    /// <summary>
    /// 是否允许记住登录状态
    /// </summary>
    public bool AllowRememberLogin { get; set; } = true;

    /// <summary>
    /// 是否启用本地登录
    /// </summary>
    public bool EnableLocalLogin { get; set; } = true;

    /// <summary>
    /// 外部提供类
    /// </summary>
    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = [];

    /// <summary>
    /// 可见的外部提供类
    /// </summary>
    public IEnumerable<ExternalProvider> VisibleExternalProviders =>
        ExternalProviders.Where(provider => !string.IsNullOrWhiteSpace(provider.DisplayName));

    /// <summary>
    /// 是否只允许外部登录
    /// </summary>
    public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
    
    /// <summary>
    /// 外部登录方案
    /// </summary>
    public string? ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.FirstOrDefault()?.AuthenticationScheme : null;
}
