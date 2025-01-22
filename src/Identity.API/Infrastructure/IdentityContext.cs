using Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Identity.API.Infrastructure;

/// <summary>
/// 认证数据库上下文(dotnet ef migrations add --context IdentityContext InitializeIdentityDatabase)
/// </summary>
public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// 认证数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }
}
