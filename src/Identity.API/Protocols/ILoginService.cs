using Microsoft.AspNetCore.Authentication;

namespace Identity.API.Protocols;

/// <summary>
/// 登录服务接口
/// </summary>
public interface ILoginService<T>
{
    /// <summary>
    /// 凭证是否有效
    /// </summary>
    /// <param name="user">用户</param>
    /// <param name="password">密码</param>
    /// <returns>凭证有效返回true，否则返回false</returns>
    Task<bool> ValidateCredentials(T user, string password);
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>登录任务</returns>
    Task SignIn(T user);
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="user">用户</param>
    /// <param name="properties">身份验证属性</param>
    /// <param name="authenticationMethod">身份验证方法</param>
    /// <returns>登录任务</returns>
    Task SignInAsync(T user, AuthenticationProperties properties, string? authenticationMethod = null);
}
