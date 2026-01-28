namespace Host.SDK
{
    /// <summary>
    /// 驱动程序健康状态信息
    /// </summary>
    public class DriverHealth
    {
        /// <summary>
        /// 是否健康
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// 最后一次通信时间
        /// </summary>
        public DateTime LastCommunicationTime { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DriverHealth()
        {
            IsHealthy = false;
            ConnectionStatus = "Disconnected";
            LastCommunicationTime = DateTime.MinValue;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isHealthy">是否健康</param>
        /// <param name="errorMessage">错误信息</param>
        public DriverHealth(bool isHealthy, string errorMessage)
        {
            IsHealthy = isHealthy;
            ConnectionStatus = isHealthy ? "Connected" : "Disconnected";
            LastCommunicationTime = DateTime.Now;
            ErrorMessage = errorMessage;
        }
    }
}