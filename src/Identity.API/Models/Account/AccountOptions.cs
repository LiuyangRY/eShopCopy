namespace Identity.API.Models.Account;

/// <summary>
/// 账号选项
/// </summary>
public class AccountOptions
{
    /// <summary>
    /// 允许本地登录
    /// </summary>
    public static bool AllowLocalLogin = true;
    
    /// <summary>
    /// 允许记住登录
    /// </summary>
    public static bool AllowRememberLogin = true;

    /// <summary>
    /// 显示注销提示
    /// </summary>
    public static bool ShowLogoutPrompt = false;
    
    /// <summary>
    /// 注销登录后自动重定向
    /// </summary>
    public static bool AutomaticRedirectAfterSignOut = true;
    
    /// <summary>
    /// 无效认证信息提示消息
    /// </summary>
    public static string InvalidCredentialsErrorMessage = "登录账号或密码无效";
}
