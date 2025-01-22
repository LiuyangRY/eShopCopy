using System.Security.Claims;
using Common.Helper;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Identity.API.Models;
using IdentityModel;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Identity.API.Services;

/// <summary>
/// 概要信息服务
/// </summary>
public class ProfileService : IProfileService
{
    /// <summary>
    /// 用户标识类型
    /// </summary>
    private readonly string _claimType = "sub";

    /// <summary>
    /// 用户管理
    /// </summary>
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// 概要信息服务构造函数
    /// </summary>
    /// <param name="userManager">用户管理</param>
    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// 获取概要数据
    /// </summary>
    /// <param name="context">概要数据请求上下文</param>
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        context.IsNotNull("概要数据请求上下文不能为空");
        context.Subject.IsNotNull("概要数据请求上下文的主体不能为空");
        var subjectId = context.Subject.Claims.FirstOrDefault(claim => claim.Type == _claimType)?.Value;
        subjectId!.IsNotNullOrWhitespace($"【{_claimType}】类型的用户id不存在");
        var user = await _userManager.FindByIdAsync(subjectId!);
        user.IsNotNull($"未找到用户，id：{subjectId}");
        context.IssuedClaims = GetClaimsFromUser(user!);
    }

    /// <summary>
    /// 是否有效
    /// </summary>
    /// <param name="context">是否有效上下文</param>
    /// <returns>是否有效判断任务</returns>
    public async Task IsActiveAsync(IsActiveContext context)
    {
        context.IsNotNull("是否有效上下文不能为空");
        context.Subject.IsNotNull("是否有效上下文的主体不能为空");
        context.IsActive = false;
        var subjectId = context.Subject.Claims.FirstOrDefault(claim => claim.Type == _claimType)?.Value;
        if (string.IsNullOrWhiteSpace(subjectId))
            return;
        var user = await _userManager.FindByIdAsync(subjectId);
        if (user is not null)
        {
            if (_userManager.SupportsUserSecurityStamp)
            {
                var securityStamp = context.Subject.Claims.FirstOrDefault(claim => claim.Type == "security_stamp")
                    ?.Value;
                if (securityStamp is not null)
                {
                    var dbSecurityStamp = await _userManager.GetSecurityStampAsync(user);
                    if (dbSecurityStamp != securityStamp)
                        return;
                }
            }

            if (!user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTime.UtcNow)
            {
                context.IsActive = true;
            }
        }
    }

    /// <summary>
    /// 获取用户标识
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>用户标识集合</returns>
    private List<Claim> GetClaimsFromUser(ApplicationUser user)
    {
        user.UserName!.IsNotNullOrWhitespace("用户标识中的用户名称不能为空");
        List<Claim> claims =
        [
            new(JwtClaimTypes.Subject, user.Id),
            new(JwtClaimTypes.PreferredUserName, user.UserName!),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
        ];
        AddClaims(claims, "name", user.Name);
        AddClaims(claims, "last_name", user.LastName);
        AddClaims(claims, "card_number", user.CardNumber);
        AddClaims(claims, "card_holder", user.CardHolderName);
        AddClaims(claims, "card_security_number", user.SecurityNumber);
        AddClaims(claims, "card_expiration", user.Expiration);
        AddClaims(claims, "address_city", user.City);
        AddClaims(claims, "address_country", user.Country);
        AddClaims(claims, "address_state", user.State);
        AddClaims(claims, "address_street", user.Street);
        AddClaims(claims, "address_zip_code", user.ZipCode);
        if (_userManager.SupportsUserEmail)
        {
            var emailConfirmed = user.EmailConfirmed ? "true" : "false";
            AddClaims(claims, JwtClaimTypes.Email, user.Email!);
            AddClaims(claims, JwtClaimTypes.EmailVerified, emailConfirmed, ClaimValueTypes.Boolean);
        }

        if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            var phoneConfirmed = user.PhoneNumberConfirmed ? "true" : "false";
            AddClaims(claims, JwtClaimTypes.PhoneNumber, user.PhoneNumber);
            AddClaims(claims, JwtClaimTypes.PhoneNumberVerified, phoneConfirmed, ClaimValueTypes.Boolean);
        }

        return claims;
    }

    /// <summary>
    /// 添加用户标识
    /// </summary>
    /// <param name="claims">用户标识列表</param>
    /// <param name="claimType">用户标识类型</param>
    /// <param name="claimValue">用户标识值</param>
    /// <param name="claimValueType">用户标识值类型</param>
    private void AddClaims(List<Claim> claims, string claimType, string claimValue, string? claimValueType = null)
    {
        claimType.IsNotNullOrWhitespace("用户标识类型不能为空");
        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            var claim = claimValueType is null
                ? new Claim(claimType, claimValue)
                : new Claim(claimType, claimValue, claimValueType);
            claims.Add(claim);
        }
    }
}
