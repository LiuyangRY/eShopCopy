using Identity.API.Models;
using Identity.API.Protocols;
using Microsoft.AspNetCore.Authentication;

namespace Identity.API.Services;

/// <summary>
/// EF登录服务
/// </summary>
public class EFLoginService : ILoginService<ApplicationUser>
{
    /// <summary>
    /// 用户管理服务
    /// </summary>
    private UserManager<ApplicationUser> _userManager;
    
    /// <summary>
    /// 登录管理服务
    /// </summary>
    private SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// EF登录服务构造函数
    /// </summary>
    /// <param name="userManager">用户管理服务</param>
    /// <param name="signInManager">登录管理服务</param>
    public EFLoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// 凭证是否有效
    /// </summary>
    /// <param name="user">用户</param>
    /// <param name="password">密码</param>
    /// <returns>凭证有效返回true，否则返回false</returns>
    public async Task<bool> ValidateCredentials(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>登录任务</returns>
    public Task SignIn(ApplicationUser user)
    {
        return _signInManager.SignInAsync(user, true);
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="user">用户</param>
    /// <param name="properties">身份验证属性</param>
    /// <param name="authenticationMethod">身份验证方法</param>
    /// <returns>登录任务</returns>
    public Task SignInAsync(ApplicationUser user, AuthenticationProperties properties, string? authenticationMethod = null)
    {
        return _signInManager.SignInAsync(user, properties, authenticationMethod);
    }
}
