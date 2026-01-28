using System;

namespace Host.SDK
{
    /// <summary>
    /// 连接状态改变事件参数
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 设备连接ID
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 新的连接状态
        /// </summary>
        public ConnectionState NewState { get; set; }

        /// <summary>
        /// 旧的连接状态
        /// </summary>
        public ConnectionState OldState { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionId">设备连接ID</param>
        /// <param name="oldState">旧的连接状态</param>
        /// <param name="newState">新的连接状态</param>
        public ConnectionStatusChangedEventArgs(string connectionId, ConnectionState oldState, ConnectionState newState)
        {
            ConnectionId = connectionId;
            OldState = oldState;
            NewState = newState;
        }
    }
}