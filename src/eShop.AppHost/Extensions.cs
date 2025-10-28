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
            var composeFileDirectoryPath = Directory.GetDirectories("../../deploy").FirstOrDefault(path => path.EndsWith("docker")) ?? ".";
            string? composeFilePath = null;
            string? workingDirectory = null;

            var existComposeFile = false;
            if (Directory.Exists(composeFileDirectoryPath))
            {
                var filePath = Path.Combine(composeFileDirectoryPath, "docker-compose.yml");
                if (File.Exists(filePath))
                {
                    workingDirectory = composeFileDirectoryPath;
                    composeFilePath = filePath;
                    existComposeFile = true;
                }
            }
            if (!existComposeFile)
            {
                throw new DirectoryNotFoundException($"docker-compose.yml 不存在，已查找以下路径: {Path.GetFullPath(composeFileDirectoryPath)}");
            }

            string env = Environment.GetEnvironmentVariable("ENV") ?? "dev";
            string envComposeFile = Path.Combine(workingDirectory!, $"docker-compose.{env}.yml");
            string envFile = Path.Combine(workingDirectory!, $".env.{env}");

            if (!File.Exists(envFile))
            {
                throw new FileNotFoundException($"环境变量文件 {envFile} 不存在", envFile);
            }

            List<string> args = [$"-f {composeFilePath}"];
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