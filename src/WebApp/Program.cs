using eShop.WebApp.Components;
using eShop.WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationServices();

var app = builder.Build();


// 配置Http请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // 默认的 HSTS（严格传输安全协议） 值为30天，生产环境下可以修改
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
app.UseRouting();
app.UseCookiePolicy();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.ConfigRouteForward();
app.MapDefaultEndpoints();
app.Run();
