using Common.Constant;
using eShop.AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddForwardedHeaders();
await builder.InitEnvironmentAsync();

var launchProfileName = GetHttpProtocolNameForEndpoint();
var connectionString = builder.Configuration.GetConnectionString("PostgresConnectionString");
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api", launchProfileName)
    .WithEnvironment("ConnectionStrings:identityDb", $"{connectionString}Database=identityDb;");

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithEnvironment("ConnectionStrings:catalogDb", $"{connectionString}Database=catalogDb;");

var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)
    .WithExternalHttpEndpoints()
    .WithReference(catalogApi);

// 回调url
var identityApiUri = identityApi.GetEndpoint(launchProfileName);
var webAppUri = webApp.GetEndpoint(launchProfileName);

// 设置环境变量
webApp.WithEnvironment(ServiceConstants.WebApp, webAppUri)
    .WithEnvironment(ServiceConstants.IdentityApiUri, identityApiUri);
identityApi.WithEnvironment(ServiceConstants.WebApp, webAppUri);
builder.Build().Run();

// 获取终结点HTTP协议名称
static string GetHttpProtocolNameForEndpoint()
{
    var envValue = Environment.GetEnvironmentVariable("ESHOP_USE_HTTP_ENDPOINTS");
    return int.TryParse(envValue, out var value) && value == 1 ? "http" : "https";
}
