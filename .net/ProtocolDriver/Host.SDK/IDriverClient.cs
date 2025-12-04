using System.Threading;
using System.Threading.Tasks;

namespace Host.SDK
{
    /// <summary>
    /// 驱动客户端通用接口，定义了与设备通信的基本操作
    /// </summary>
    public interface IDriverClient
    {
        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <param name="ipAddress">设备IP地址</param>
        /// <param name="port">设备端口</param>
        /// <param name="unitId">设备单元ID</param>
        /// <param name="token">取消令牌</param>
        /// <returns>连接是否成功</returns>
        Task<bool> ConnectAsync(string ipAddress, int port, byte unitId, CancellationToken token = default);

        /// <summary>
        /// 断开与设备的连接
        /// </summary>
        /// <returns>断开是否成功</returns>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// 检查连接状态
        /// </summary>
        /// <returns>连接是否正常</returns>
        bool IsConnected { get; }

        /// <summary>
        /// 读取设备数据
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <typeparam name="TRequest">请求实体类型</typeparam>
        /// <param name="request">读取请求实体</param>
        /// <param name="token">取消令牌</param>
        /// <returns>读取的数据</returns>
        Task<T[]> Read<T, TRequest>(TRequest request, CancellationToken token = default) where TRequest : ReadRequestBase;

        /// <summary>
        /// 写入设备数据
        /// </summary>
        /// <typeparam name="T">写入数据类型</typeparam>
        /// <typeparam name="TRequest">请求实体类型</typeparam>
        /// <param name="request">写入请求实体</param>
        /// <param name="token">取消令牌</param>
        /// <returns>写入是否成功</returns>
        Task<bool> Write<TRequest>(TRequest request, CancellationToken token = default) where TRequest : WriteRequestBase;
    }
}