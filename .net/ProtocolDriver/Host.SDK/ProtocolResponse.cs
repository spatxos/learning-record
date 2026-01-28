using System.Collections.Generic;

namespace Host.SDK
{
    /// <summary>
    /// 协议响应类
    /// </summary>
    public record ProtocolResponse(bool Success, byte[]? Payload = null, IDictionary<string, object>? Parsed = null, string? Error = null);
}