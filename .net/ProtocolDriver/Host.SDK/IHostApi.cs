using System.Threading;using System.Threading.Tasks;

namespace Host.SDK
{
    /// <summary>
    /// Host API接口
    /// </summary>
    public interface IHostApi
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">事件数据</param>
        /// <param name="token">取消令牌</param>
        /// <returns>任务</returns>
        Task PublishEventAsync(string eventName, object data, CancellationToken token = default);
    }
}