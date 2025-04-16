using Common.Constant;
using Identity.API.Models;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.API.Infrastructure;

/// <summary>
/// 认证数据库上下文种子
/// </summary>
public class IdentityContextSeed(ILogger<IdentityContextSeed> logger
, UserManager<ApplicationUser> userManager
, IOpenIddictApplicationManager openIddictApplicationManager
, IOpenIddictScopeManager openIddictScopeManager
, IWebHostEnvironment environment
, IConfiguration configuration)
    : IDbSeeder<IdentityContext>
{
    /// <summary>
    /// 创建种子数据
    /// </summary>
    /// <param name="context">认证数据库上下文</param>
    public async Task SeedAsync(IdentityContext context)
    {
        await InitApplicationUserAsync();
        await InitApplycationsAsync();
        await InitScopesAsync();
    }

    /// <summary>
    /// 初始化应用程序用户 
    /// </summary>
    /// <returns>初始化任务</returns>
    private async Task InitApplicationUserAsync()
    {
        var clientIds = new List<string> { ServiceConstants.ApiTest, ServiceConstants.WebApp };
        if (await userManager.FindByNameAsync("admin") is not ApplicationUser admin)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@email.com",
                ClientIds = clientIds
            };
            var result = await userManager.CreateAsync(admin, "Pass123$");
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("管理员创建成功");
            }
        }
    }

    /// <summary>
    /// 初始化应用程序
    /// </summary>
    /// <returns>初始化任务</returns> 
    private async Task InitApplycationsAsync()
    {
        if (await openIddictApplicationManager.FindByClientIdAsync(ServiceConstants.WebApp) is null)
        {
            var callbackUri = configuration.GetRequiredValue(ServiceConstants.WebApp);
            var application = new OpenIddictApplicationDescriptor
            {
                ClientId = ServiceConstants.WebApp,
                DisplayName = ServiceConstants.WebApp,
                ApplicationType = ApplicationTypes.Web,
                ClientType = ClientTypes.Confidential,
                ClientSecret = $"{ServiceConstants.WebApp}SecretKey",
                RedirectUris = { 
                    new Uri($"{callbackUri}/signin-oidc")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + ServiceConstants.WebApp,
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            };
            await openIddictApplicationManager.CreateAsync(application);
        }
        if (environment.IsDevelopment())
        {
            if (await openIddictApplicationManager.FindByClientIdAsync(ServiceConstants.ApiTest) is null)
            {
                var application = new OpenIddictApplicationDescriptor
                {
                    ClientId = ServiceConstants.ApiTest,
                    DisplayName = ServiceConstants.ApiTest,
                    ApplicationType = ApplicationTypes.Native,
                    ClientType = ClientTypes.Public,
                    RedirectUris =
                    {
                        new Uri($"http://localhost:5000/callback/{ServiceConstants.ApiTest}")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Introspection,
                        Permissions.Endpoints.Revocation,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.GrantTypes.Password,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + ServiceConstants.ApiTest,
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                };
                await openIddictApplicationManager.CreateAsync(application);
            }
        }
    }

    /// <summary>
    /// 初始化作用域
    /// </summary>
    /// <returns>初始化任务</returns> 
    private async Task InitScopesAsync()
    {
        if (await openIddictScopeManager.FindByNameAsync(ServiceConstants.WebApp) is null)
        {
            var scope = new OpenIddictScopeDescriptor
            {
                Name = ServiceConstants.WebApp,
                Resources =
                    {
                        ServiceConstants.WebApp
                    }
            };
            await openIddictScopeManager.CreateAsync(scope);
        }
        if (environment.IsDevelopment())
        {
            if (await openIddictScopeManager.FindByNameAsync(ServiceConstants.ApiTest) is null)
            {
                var scope = new OpenIddictScopeDescriptor
                {
                    Name = ServiceConstants.ApiTest,
                    Resources =
                    {
                        ServiceConstants.ApiTest
                    }
                };
                await openIddictScopeManager.CreateAsync(scope);
            }
        }
    }
}
