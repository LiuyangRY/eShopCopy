using Common.Constant;
using eShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddForwardedHeaders();

var launchProfileName = GetHttpProtocolNameForEndpoint();

var identityDbConnection = builder.AddConnectionString("IdentityDb", "ConnectionStrings__IdentityDb");
var catalogDbConnection = builder.AddConnectionString("CatalogDb", "ConnectionStrings__CatalogDb");

var identityApi = builder.AddProject<Projects.Identity_API>("identity-api", launchProfileName)
    .WithReference(identityDbConnection);

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api", launchProfileName)
    .WithReference(catalogDbConnection);

var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)
    .WithExternalHttpEndpoints()
    .WaitFor(identityApi)
    .WaitFor(catalogApi)
    .WithReference(identityApi)
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
