using eShop.WebApp.Components;
using eShop.WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// 配置Http请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // 默认的 HSTS（严格传输安全协议） 值为30天，生产环境下可以修改
    app.UseHsts();
}

app.UseAntiforgery();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapForwarder("/product-images/{id}", ServiceConstant.CatalogApiUrl, "/api/catalog/{id}/pic");
app.Run();
