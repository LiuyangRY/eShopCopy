using eShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddForwardedHeaders();

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankana/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalog");

var launchProfileName = GetHttpProtocolNameForEndpoint();

var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)
    .WithExternalHttpEndpoints();

// 连接回调url
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint(launchProfileName));

builder.Build().Run();

// 获取终结点HTTP协议名称
static string GetHttpProtocolNameForEndpoint()
{
    var envValue = Environment.GetEnvironmentVariable("ESHOP_USE_HTTP_ENDPOINTS");
    return int.TryParse(envValue, out var value) && value == 1 ? "http" : "https";
}
