var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();
app.MapDefaultEndpoints();
app.Run();
