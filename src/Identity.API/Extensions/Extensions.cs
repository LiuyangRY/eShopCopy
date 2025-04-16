using System.Security.Cryptography.X509Certificates;
using Common.Helper;
using Identity.API.Configuration;
using Identity.API.Infrastructure;
using Identity.API.Models;
using Quartz;

namespace Identity.API.Extensions;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加应用程序服务
    /// </summary>
    /// <param name="webBuilder">Web应用程序构建类</param>
    /// <returns>Web应用程序构建类</returns>
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder webBuilder)
    {
        webBuilder.Services.AddControllersWithViews();
        webBuilder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        // webBuilder.Services.AddQuartzHostedService(options =>
        // {
        //     options.WaitForJobsToComplete = true;
        // });
        webBuilder.AddNpgsqlDbContext<IdentityContext>("identityDb", configureDbContextOptions: options =>
        {
            options.UseOpenIddict();
        });

        var isDevelopment = webBuilder.Environment.IsDevelopment();
        if (isDevelopment)
        {
            webBuilder.Services.AddMigration<IdentityContext, IdentityContextSeed>();
        }
        webBuilder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();
        webBuilder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetTokenEndpointUris("/connect/token")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetUserInfoEndpointUris("/connect/userinfo");

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowClientCredentialsFlow()
                    .AllowPasswordFlow();

                options.RequireProofKeyForCodeExchange();

                if (isDevelopment)
                {
                    options.AddDevelopmentSigningCertificate()
                       .AddDevelopmentEncryptionCertificate();
                }
                else
                {
                    var configuration = webBuilder.Configuration;
                    var certificateSettings = configuration.GetSection("Certificates");
                    var signingCertificate = GetCertificateByThumbprint(certificateSettings.GetValue<string>("Signing:Thumbprint"));
                    var encryptionCertificate = GetCertificateByFile(certificateSettings.GetValue<string>("Encryption:FilePath"), certificateSettings.GetValue<string>("Encryption:Password"));
                    options.AddSigningCertificate(signingCertificate);
                    options.AddEncryptionCertificate(encryptionCertificate);
                }
                options.RegisterScopes(Config.GetScopes());
                options.SetAccessTokenLifetime(TimeSpan.FromHours(Config.AccessTokenLifeTimeInHours));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(Config.RefreshTokenLifeTimeInDays));
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableUserInfoEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        // webBuilder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        //     policy.AllowAnyHeader()
        //         .AllowAnyMethod()
        //         .WithOrigins(ServiceConstants.WebAppUrl)));
        return webBuilder;
    }

    /// <summary>
    /// 根据指纹获取证书
    /// </summary>
    /// <param name="thumbprint">指纹</param>
    /// <returns>证书</returns>
    private static X509Certificate2 GetCertificateByThumbprint(string? thumbprint)
    {
        thumbprint.IsNotNullOrWhitespace("证书指纹不能为空");
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        var cert = store.Certificates.Find(
            X509FindType.FindByThumbprint,
            thumbprint!,
            validOnly: false)
            .OfType<X509Certificate2>()
            .FirstOrDefault();

        return cert ?? throw new InvalidOperationException($"签名证书未找到，指纹: {thumbprint}");
    }

    /// <summary>
    /// 根据文件获取证书
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="password">证书密码</param>
    /// <returns>证书</returns>
    private static X509Certificate2 GetCertificateByFile(string? filePath, string? password)
    {
        filePath.IsNotNullOrWhitespace("证书文件路径不能为空");
        password.IsNotNullOrWhitespace("证书密码不能为空");
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"加密证书文件未找到: {filePath}");
        return X509Certificate2.CreateFromEncryptedPemFile(
                filePath,
                password,
                Path.ChangeExtension(filePath, ".key"));
    }
}
