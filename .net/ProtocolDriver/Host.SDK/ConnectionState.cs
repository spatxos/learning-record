namespace Host.SDK
{
    /// <summary>
    /// 连接状态枚举
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// 已断开连接
        /// </summary>
        Disconnected,
        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected,
        /// <summary>
        /// 连接错误
        /// </summary>
        Error
    }
}