using System;using System.Collections.Generic;using System.Threading;using System.Threading.Tasks;

namespace Host.SDK
{
    /// <summary>
    /// 协议驱动接口，所有协议插件必须实现此接口
    /// </summary>
    public interface IProtocolDriver : IDisposable
    {
        /// <summary>
        /// 协议名称
        /// </summary>
        string ProtocolName { get; }
        
        /// <summary>
        /// 版本号
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 初始化：Host 为插件注入运行时能力
        /// </summary>
        /// <param name="context">驱动上下文</param>
        /// <param name="token">取消令牌</param>
        Task InitializeAsync(DriverContext context, CancellationToken token = default);

        /// <summary>
        /// 根据输入构造请求
        /// </summary>
        /// <param name="requestModel">请求模型</param>
        /// <returns>请求字节数组</returns>
        byte[] BuildRequest(object requestModel);

        /// <summary>
        /// 主动发起请求/执行命令
        /// </summary>
        /// <param name="request">请求字节数组</param>
        /// <param name="token">取消令牌</param>
        /// <returns>执行结果</returns>
        Task<DriverResult> ExecuteAsync(byte[] request, CancellationToken token = default);

        /// <summary>
        /// 从原始响应解析到统一数据结构
        /// </summary>
        /// <param name="response">响应字节数组</param>
        /// <returns>解析结果</returns>
        DriverParseResult ParseResponse(byte[] response);

        /// <summary>
        /// 可选：插件自身的健康检查
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>健康检查结果</returns>
        Task<DriverHealth> CheckHealthAsync(CancellationToken token = default);
    }
}