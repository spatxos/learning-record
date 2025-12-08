using Host.SDK;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace S7
{
    public class S7Driver : IProtocolDriver
    {
        private DriverContext _context = null!;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private string _ipAddress = "127.0.0.1";
        private int _port = 102;
        private byte[] _pduHeader = new byte[4] { 0x32, 0x01, 0x00, 0x00 };
        private ushort _pduLength = 0;
        private byte _messageType = 0x03;
        private byte _messageFunction = 0x00;
        private ushort _messageLength = 0;
        private byte _errorClass = 0x00;
        private byte _errorCode = 0x00;
        private byte[] _reserve = new byte[2] { 0x00, 0x00 };
        private byte _protocolId = 0x11;
        private byte _rosctr = 0x01;
        private byte _pduRef = 0x01;

        public string ProtocolName => "S7"; 
        public string Version => "1.0.0";

        public PluginMetadata Metadata => throw new NotImplementedException();

        public async Task InitializeAsync(DriverContext context, CancellationToken token = default)
        {
            _context = context;
            _context.Logger.LogInformation("S7 driver initialized");

            // 从配置读取连接信息
            _ipAddress = _context.Config.GetSection("S7:IpAddress").Value ?? _ipAddress;
            if (!int.TryParse(_context.Config.GetSection("S7:Port").Value, out int value))
            {
                value = _port;
            }
            _port = value;

            // 建立连接
            await ConnectAsync(token);
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            try
            {
                _client = _context.TransportFactory.CreateTcpClient();
                await _client.ConnectAsync(_ipAddress, _port, token);
                _stream = _client.GetStream();
                _context.Logger.LogInformation("Connected to S7 server: {IpAddress}:{Port}", _ipAddress, _port);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to connect to S7 server: {IpAddress}:{Port}", _ipAddress, _port);
                throw;
            }
        }

        public byte[] BuildRequest(object requestModel)
        {
            if (requestModel is not S7Request request)
                throw new ArgumentException("Invalid request model type", nameof(requestModel));

            _context.Logger.LogDebug("BuildRequest: {Command} - {AreaCode}{Address}:{Count}", 
                request.Command, request.AreaCode, request.Address, request.Count);

            // 保存当前请求，用于解析响应
            S7Request.Current = request;

            // 构建S7协议请求帧
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // TCP头部
                writer.Write(_pduHeader);

                // 参数部分
                writer.Write(_protocolId);
                writer.Write(_rosctr);
                writer.Write(_pduRef);
                writer.Write(_reserve);
                writer.Write((ushort)0x0000); // 参数长度
                writer.Write((ushort)0x0000); // 数据长度

                // 建立连接请求
                writer.Write(_messageType);
                writer.Write(_messageFunction);
                writer.Write((ushort)0x000e); // 消息长度
                writer.Write(_errorClass);
                writer.Write(_errorCode);
                writer.Write((byte)0x00); // 保留
                writer.Write((byte)0x04); // 数据长度
                writer.Write((byte)0x00); // 协议类型 (PG)
                writer.Write((byte)0x01); // 连接类型 (ISO)
                writer.Write((byte)0x00); // 保留
                writer.Write((byte)0x01); // 保留

                // 读写请求
                writer.Write((byte)0x04); // 功能码
                writer.Write((byte)0x01); // 地址数量
                writer.Write(GetAreaCode(request.AreaCode)); // 区域代码
                writer.Write((byte)(request.Address >> 24)); // 地址高位
                writer.Write((byte)(request.Address >> 16));
                writer.Write((byte)(request.Address >> 8));
                writer.Write((byte)request.Address); // 地址低位
                writer.Write((ushort)request.Count); // 数据长度

                var requestData = ms.ToArray();
                
                // 计算并更新PDU长度
                _pduLength = (ushort)(requestData.Length - 2);
                requestData[2] = (byte)(_pduLength >> 8);
                requestData[3] = (byte)_pduLength;

                return requestData;
            }
        }

        private byte GetAreaCode(string areaCode)
        {
            switch (areaCode.ToUpper())
            {
                case "DB":
                    return 0x84;
                case "M":
                    return 0x83;
                case "I":
                    return 0x81;
                case "Q":
                    return 0x82;
                default:
                    return 0x84;
            }
        }

        public async Task<DriverResult> ExecuteAsync(byte[] request, CancellationToken token = default)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    await ConnectAsync(token);
                }

                // 发送请求
                await _stream!.WriteAsync(request, token);
                await _stream.FlushAsync(token);
                _context.Logger.LogDebug("Sent S7 request: {Request}", BitConverter.ToString(request));

                // 接收响应
                var response = await ReadResponseAsync(token);
                _context.Logger.LogDebug("Received S7 response: {Response}", BitConverter.ToString(response));

                return new DriverResult(true, response);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "ExecuteAsync failed");
                return new DriverResult(false, Array.Empty<byte>(), ex.Message);
            }
        }

        private async Task<byte[]> ReadResponseAsync(CancellationToken token)
        {
            // 先读取TCP头部
            var header = new byte[4];
            await ReadExactAsync(_stream!, header, token);

            // 计算响应长度
            int responseLength = (header[2] << 8) | header[3];
            responseLength += 2; // 加上头部长度

            // 读取响应数据
            var data = new byte[responseLength];
            await ReadExactAsync(_stream!, data, token);

            // 合并所有部分
            var fullResponse = new List<byte>(header);
            fullResponse.AddRange(data);

            return fullResponse.ToArray();
        }

        private async Task ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken token)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer, totalRead, buffer.Length - totalRead, token);
                if (read == 0)
                    throw new EndOfStreamException();
                totalRead += read;
            }
        }

        public DriverParseResult ParseResponse(byte[] response)
        {
            // 解析S7协议响应
            var tags = new string[] { "S7" };
            var data = new Dictionary<string, object>();

            try
            {
                using (var ms = new MemoryStream(response))
                using (var reader = new BinaryReader(ms))
                {
                    // 跳过TCP头部
                    reader.ReadBytes(4);

                    // 读取参数部分
                    _protocolId = reader.ReadByte();
                    _rosctr = reader.ReadByte();
                    _pduRef = reader.ReadByte();
                    reader.ReadBytes(2); // 保留
                    var paramLength = reader.ReadUInt16();
                    var dataLength = reader.ReadUInt16();

                    // 读取消息类型和功能
                    _messageType = reader.ReadByte();
                    _messageFunction = reader.ReadByte();
                    _messageLength = reader.ReadUInt16();
                    _errorClass = reader.ReadByte();
                    _errorCode = reader.ReadByte();

                    if (_errorClass != 0 || _errorCode != 0)
                    {
                        data["Error"] = $"Class: {_errorClass}, Code: {_errorCode}";
                        return new DriverParseResult(tags, data);
                    }

                    // 跳过其他头部信息
                    reader.ReadBytes(10);

                    // 解析数据
                    var s7Request = S7Request.Current;
                    if (s7Request != null)
                    {
                        var values = new List<ushort>();
                        for (int i = 0; i < s7Request.Count; i++)
                        {
                            values.Add(reader.ReadUInt16());
                        }
                        data["Values"] = values;
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "ParseResponse failed");
                data["Error"] = ex.Message;
            }

            return new DriverParseResult(tags, data);
        }

        public Task<DriverHealth> CheckHealthAsync(CancellationToken token = default)
        {
            try
            {
                if (_client != null && _client.Connected)
                {
                    return Task.FromResult(new DriverHealth(true, "Connected to S7 server"));
                }
                return Task.FromResult(new DriverHealth(false, "Not connected to S7 server"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new DriverHealth(false, ex.Message));
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
            _context.Logger.LogInformation("S7 driver disposed");
        }

        public Task<IProtocolConnection> CreateConnectionAsync(IDictionary<string, string> settings, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// S7协议请求模型
    /// </summary>
    public class S7Request
    {
        public static S7Request? Current { get; set; }
        public string Command { get; set; } = "Read";
        public string AreaCode { get; set; } = "DB";
        public uint Address { get; set; } = 0;
        public ushort Count { get; set; } = 1;
    }
}