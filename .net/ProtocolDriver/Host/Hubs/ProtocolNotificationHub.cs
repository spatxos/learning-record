using Host.SDK;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Host.Hubs
{
    /// <summary>
    /// 协议通知Hub，用于向客户端推送实时通知
    /// </summary>
    public class ProtocolNotificationHub : Hub
    {
        /// <summary>
        /// 连接建立时的处理
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", new { ConnectionId = Context.ConnectionId });
        }

        /// <summary>
        /// 连接断开时的处理
        /// </summary>
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 订阅设备通知
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        public async Task SubscribeToDevice(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        }

        /// <summary>
        /// 取消订阅设备通知
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        public async Task UnsubscribeFromDevice(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        }

        /// <summary>
        /// 订阅所有设备通知
        /// </summary>
        public async Task SubscribeToAllDevices()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllDevices");
        }

        /// <summary>
        /// 取消订阅所有设备通知
        /// </summary>
        public async Task UnsubscribeFromAllDevices()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllDevices");
        }

        /// <summary>
        /// 通知设备连接状态变更
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="connectionId">连接ID</param>
        /// <param name="newState">新状态</param>
        public async Task NotifyConnectionStatusChanged(string deviceId, string connectionId, ConnectionState newState)
        {
            await Clients.Group($"Device_{deviceId}").SendAsync("ConnectionStatusChanged", new
            {
                DeviceId = deviceId,
                ConnectionId = connectionId,
                NewState = newState,
                Timestamp = System.DateTime.UtcNow
            });
            await Clients.Group("AllDevices").SendAsync("ConnectionStatusChanged", new
            {
                DeviceId = deviceId,
                ConnectionId = connectionId,
                NewState = newState,
                Timestamp = System.DateTime.UtcNow
            });
        }

        /// <summary>
        /// 通知请求执行结果
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="requestId">请求ID</param>
        /// <param name="response">协议响应</param>
        public async Task NotifyRequestResult(string deviceId, string requestId, ProtocolResponse response)
        {
            await Clients.Group($"Device_{deviceId}").SendAsync("RequestResult", new
            {
                DeviceId = deviceId,
                RequestId = requestId,
                Response = response
            });
            await Clients.Group("AllDevices").SendAsync("RequestResult", new
            {
                DeviceId = deviceId,
                RequestId = requestId,
                Response = response
            });
        }

        /// <summary>
        /// 通知插件加载状态变更
        /// </summary>
        /// <param name="pluginName">插件名称</param>
        /// <param name="status">状态</param>
        /// <param name="message">消息</param>
        public async Task NotifyPluginStatusChanged(string pluginName, string status, string message)
        {
            await Clients.All.SendAsync("PluginStatusChanged", new
            {
                PluginName = pluginName,
                Status = status,
                Message = message,
                Timestamp = System.DateTime.UtcNow
            });
        }

        /// <summary>
        /// 发送系统通知
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="level">通知级别</param>
        public async Task SendSystemNotification(string title, string message, string level = "Info")
        {
            await Clients.All.SendAsync("SystemNotification", new
            {
                Title = title,
                Message = message,
                Level = level,
                Timestamp = System.DateTime.UtcNow
            });
        }
    }
}