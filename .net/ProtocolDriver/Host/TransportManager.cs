using Host.SDK;
using System.Net.Sockets;

namespace Host
{
    /// <summary>
    /// 传输管理器，实现通信通道的管理
    /// </summary>
    public class TransportManager : ITransportFactory
    {
        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <returns>TCP客户端</returns>
        public TcpClient CreateTcpClient()
        {
            return new TcpClient();
        }

        /// <summary>
        /// 创建UDP客户端
        /// </summary>
        /// <returns>UDP客户端</returns>
        public UdpClient CreateUdpClient()
        {
            return new UdpClient();
        }
    }
}