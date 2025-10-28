using Aspire.Hosting.Lifecycle;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace eShop.AppHost;

/// <summary>
/// 扩展类
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 添加一个钩子为所有应用程序添加 ASPNETCORE_FORWARDEDHEADERS_ENABLED 环境变量
    /// </summary>
    public static IDistributedApplicationBuilder AddForwardedHeaders(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    /// <summary>
    /// 初始化环境
    /// </summary>
    public static async Task<IDistributedApplicationBuilder> InitEnvironmentAsync(this IDistributedApplicationBuilder builder)
    {
        await EnsureDockerContainersReadyAsync();
        await builder.EnsurePgVectorExtensionReady();
        return builder;
    }

    /// <summary>
    /// 添加转发头钩子
    /// </summary>
    private class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
        {
            foreach (var resource in appModel.GetProjectResources())
            {
                resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
                {
                    context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = "true";
                }));
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 确保docker容器已准备好
    /// </summary> 
    private static async Task EnsureDockerContainersReadyAsync()
    {
        using var dockerClient = new DockerClientConfiguration().CreateClient();
    
        // 检查postgres容器
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["name"] = new Dictionary<string, bool> { ["postgres"] = true }
            }
        });
    
        if (!containers.Any(c => c.State == "running"))
        {
            string? workingDirectory = null;
            string? composeFilePath = null;
            var currentDir = Directory.GetCurrentDirectory();
            
            // 查找docker-compose.yml文件
            for (int i = 0; i < 5; i++)
            {
                // 首先检查当前目录下的deploy/docker子目录
                var deployCandidate = Path.Combine(currentDir, "deploy", "docker", "docker-compose.yml");
                if (File.Exists(deployCandidate))
                {
                    workingDirectory = Path.Combine(currentDir, "deploy", "docker");
                    composeFilePath = deployCandidate;
                    break;
                }
                
                // 然后检查当前目录下的docker-compose.yml
                var candidate = Path.Combine(currentDir, "docker-compose.yml");
                if (File.Exists(candidate))
                {
                    workingDirectory = currentDir;
                    composeFilePath = candidate;
                    break;
                }
                
                currentDir = Directory.GetParent(currentDir)?.FullName ?? "";
                if (string.IsNullOrEmpty(currentDir)) break;
            }
            
            if (string.IsNullOrWhiteSpace(composeFilePath))
            {
                throw new FileNotFoundException("docker-compose.yml 不存在，已查找当前及最多5级父目录。");
            }
            
            string env = Environment.GetEnvironmentVariable("ENV") ?? "dev";
            string envComposeFile = Path.Combine(workingDirectory!, $"docker-compose.{env}.yml");
            string envFile = Path.Combine(workingDirectory!, $".env.{env}");
            
            if (!File.Exists(envFile))
            {
                throw new FileNotFoundException($"环境变量文件 {envFile} 不存在", envFile);
            }
            
            List<string> args = new();
            args.Add($"-f {composeFilePath}");
            if (File.Exists(envComposeFile))
            {
                args.Add($"-f {envComposeFile}");
            }
            args.Add($"--env-file {envFile}");
            args.Add("up -d");
            
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = string.Join(" ", args),
                    WorkingDirectory = workingDirectory!,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 确保pgvector扩展已准备好 
    /// </summary>
    /// <param name="builder">分布式应用程序生成器</param>
    /// <returns>分布式应用程序生成器</returns>
    public static async Task<IDistributedApplicationBuilder> EnsurePgVectorExtensionReady(this IDistributedApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("PostgresConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("数据库连接字符串不能为空");
        }
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", connection);
        await command.ExecuteNonQueryAsync();
        return builder;
    }
}