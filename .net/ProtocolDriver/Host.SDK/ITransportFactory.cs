using System.Net.Sockets;

namespace Host.SDK
{
    /// <summary>
    /// 传输工厂接口
    /// </summary>
    public interface ITransportFactory
    {
        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <returns>TCP客户端</returns>
        TcpClient CreateTcpClient();

        /// <summary>
        /// 创建UDP客户端
        /// </summary>
        /// <returns>UDP客户端</returns>
        UdpClient CreateUdpClient();
    }
}