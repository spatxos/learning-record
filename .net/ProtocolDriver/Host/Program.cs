using Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册核心服务
builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<TransportManager>();
builder.Services.AddSingleton<HostApi>();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// 启动插件管理器
var pluginManager = app.Services.GetRequiredService<PluginManager>();
await pluginManager.InitializeAsync();

app.Run();