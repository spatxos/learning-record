using System.Collections.Generic;

namespace Host.SDK
{
    /// <summary>
    /// 协议请求类
    /// </summary>
    public record ProtocolRequest(string Action, IDictionary<string, string> Props, byte[]? Payload = null);
}