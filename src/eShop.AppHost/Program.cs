using eShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddForwardedHeaders();

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");

var launchProfileName = GetHttpProtocolNameForEndpoint();

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb);

var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi);

// 连接回调url
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint(launchProfileName));

builder.Build().Run();

// 获取终结点HTTP协议名称
static string GetHttpProtocolNameForEndpoint()
{
    var envValue = Environment.GetEnvironmentVariable("ESHOP_USE_HTTP_ENDPOINTS");
    return int.TryParse(envValue, out var value) && value == 1 ? "http" : "https";
}
