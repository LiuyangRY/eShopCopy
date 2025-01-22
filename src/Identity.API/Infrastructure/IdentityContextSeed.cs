using Identity.API.Models;

namespace Identity.API.Infrastructure;

/// <summary>
/// 认证数据库上下文种子
/// <param name="logger">日志</param>
/// <param name="userManager">用户管理</param>
/// </summary>
public class IdentityContextSeed(ILogger<IdentityContextSeed> logger, UserManager<ApplicationUser> userManager)
    : IDbSeeder<IdentityContext>
{
    /// <summary>
    /// 创建种子数据
    /// </summary>
    /// <param name="context">认证数据库上下文</param>
    public async Task SeedAsync(IdentityContext context)
    {
        var alice = await userManager.FindByNameAsync("alice");
        if (alice is null)
        {
            alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
                CardHolderName = "Alice Smith",
                CardNumber = "XXXXXXXXXXXX1881",
                CardType = 1,
                City = "Redmond",
                Country = "U.S.",
                Expiration = "12/24",
                Id = Guid.NewGuid().ToString(),
                LastName = "Smith",
                Name = "Alice",
                PhoneNumber = "1234567890",
                ZipCode = "98052",
                State = "WA",
                Street = "15703 NE 61st Ct",
                SecurityNumber = "123"
            };
            var result = await userManager.CreateAsync(alice, "Pass123$");
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("用户【alice】创建成功");
            }
        }

        var bob = await userManager.FindByNameAsync("bob");

        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true,
                CardHolderName = "Bob Smith",
                CardNumber = "XXXXXXXXXXXX1881",
                CardType = 1,
                City = "Redmond",
                Country = "U.S.",
                Expiration = "12/24",
                Id = Guid.NewGuid().ToString(),
                LastName = "Smith",
                Name = "Bob",
                PhoneNumber = "1234567890",
                ZipCode = "98052",
                State = "WA",
                Street = "15703 NE 61st Ct",
                SecurityNumber = "456"
            };

            var result = await userManager.CreateAsync(bob, "Pass123$");

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("用户【bob】创建成功");
            }
        }
    }
}
