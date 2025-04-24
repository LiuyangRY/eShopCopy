namespace Identity.API.Infrastructure;

/// <summary>
/// 认证数据库上下文种子
/// </summary>
public class IdentityContextSeed()
    : IDbSeeder<IdentityContext>
{
    /// <summary>
    /// 创建种子数据
    /// </summary>
    /// <param name="context">认证数据库上下文</param>
    public async Task SeedAsync(IdentityContext context)
    {
        await Task.Delay(1);
    }
}
