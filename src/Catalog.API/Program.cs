using eShop.Catalog.API.Apis;
using eShop.Catalog.API.Extensions;
using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();
var withApiVersioning = builder.Services.AddApiVersioning();
builder.AddDefaultOpenApi(withApiVersioning);
var app = builder.Build();
app.UseStatusCodePages();

app.NewVersionedApi("Catalog")
    .MapCatalogApiV1();
app.NewVersionedApi("Brand")
    .MapBrandApiV1();
app.NewVersionedApi("Type")
    .MapTypeApiV1();
app.UseDefaultOpenApi();
app.MapDefaultEndpoints();
app.Run();
