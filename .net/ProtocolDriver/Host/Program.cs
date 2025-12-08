using Host;
using Host.Hubs;
using Host.PluginRuntime;
using Host.SDK;
using Host.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 配置详细日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// 注册核心服务
builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<TransportManager>();
builder.Services.AddSingleton<ITransportFactory, TransportManager>();
builder.Services.AddSingleton<HostApi>();
builder.Services.AddSingleton<IHostApi, HostApi>();
builder.Services.AddSingleton<ProtocolNotificationHub>();
builder.Services.AddSingleton<SqliteStore>(sp => new SqliteStore("protocol.db"));
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<SingletonConnectionManager>();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapHub<ProtocolNotificationHub>("/notificationhub");

// 启动插件管理器
var pluginManager = app.Services.GetRequiredService<PluginManager>();
await pluginManager.InitializeAsync();

// 初始化单例连接管理器
var singletonConnectionManager = app.Services.GetRequiredService<SingletonConnectionManager>();
await singletonConnectionManager.InitializeAsync();

app.Run();