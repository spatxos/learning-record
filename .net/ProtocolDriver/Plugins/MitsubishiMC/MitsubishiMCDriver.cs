using Host.SDK;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MitsubishiMC
{
    public class MitsubishiMCDriver : IProtocolDriver
    {
        private DriverContext _context = null!;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private string _ipAddress = "127.0.0.1";
        private int _port = 5007;
        private byte _unitId = 0;

        public string ProtocolName => "MitsubishiMC";
        public string Version => "1.0.0";

        public async Task InitializeAsync(DriverContext context, CancellationToken token = default)
        {
            _context = context;
            _context.Logger.LogInformation("Mitsubishi MC driver initialized");

            // 从配置读取连接信息
            _ipAddress = _context.Config.GetValue<string>("MitsubishiMC:IpAddress") ?? _ipAddress;
            _port = _context.Config.GetValue<int>("MitsubishiMC:Port") ?? _port;
            _unitId = _context.Config.GetValue<byte>("MitsubishiMC:UnitId") ?? _unitId;

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
                _context.Logger.LogInformation("Connected to Mitsubishi MC server: {IpAddress}:{Port}", _ipAddress, _port);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to connect to Mitsubishi MC server: {IpAddress}:{Port}", _ipAddress, _port);
                throw;
            }
        }

        public byte[] BuildRequest(object requestModel)
        {
            if (requestModel is not MCRequest request)
                throw new ArgumentException("Invalid request model type", nameof(requestModel));

            _context.Logger.LogDebug("BuildRequest: {CommandCode} - {DataType}{Address}:{Count}", 
                request.CommandCode, request.DataType, request.StartAddress, request.Count);

            // 保存当前请求，用于解析响应
            MCRequest.Current = request;

            // 构建MC协议请求帧
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Frame Header
                writer.Write((ushort)0x5000); // SubHeader type
                writer.Write((ushort)0x0000); // Serial number
                writer.Write((byte)0xff);     // Network number
                writer.Write((byte)0xff);     // PC number
                writer.Write((byte)_unitId);  // Unit number
                writer.Write((ushort)0x0000); // Reserved

                // Sub Header
                writer.Write((ushort)0x0000); // Service ID
                writer.Write((ushort)0x0000); // Reserved

                // Command
                writer.Write((ushort)request.CommandCode); // Command code
                writer.Write((ushort)0x0000);             // Subcommand code
                writer.Write((byte)0x00);                 // Timer

                // Parameter
                writer.Write((byte)request.DataType);     // Data type
                writer.Write((ushort)request.StartAddress >> 8); // Start address (high)
                writer.Write((ushort)request.StartAddress & 0xFF); // Start address (low)
                writer.Write((ushort)request.Count);      // Number of items

                return ms.ToArray();
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
                _context.Logger.LogDebug("Sent MC request: {Request}", BitConverter.ToString(request));

                // 接收响应
                var response = await ReadResponseAsync(token);
                _context.Logger.LogDebug("Received MC response: {Response}", BitConverter.ToString(response));

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
            // 先读取响应头
            var header = new byte[24];
            await ReadExactAsync(_stream!, header, token);

            // 解析帧长度（从响应头中获取）
            using (var ms = new MemoryStream(header))
            using (var reader = new BinaryReader(ms))
            {
                reader.ReadUInt16(); // SubHeader type
                reader.ReadUInt16(); // Serial number
                reader.ReadByte();   // Network number
                reader.ReadByte();   // PC number
                reader.ReadByte();   // Unit number
                reader.ReadUInt16(); // Reserved

                // 读取响应数据
                var response = new List<byte>(header);
                var dataLength = header.Length; // 简化实现，实际应从响应头计算数据长度

                if (dataLength > header.Length)
                {
                    var data = new byte[dataLength - header.Length];
                    await ReadExactAsync(_stream!, data, token);
                    response.AddRange(data);
                }

                return response.ToArray();
            }
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
            // 解析MC协议响应
            var tags = new string[] { "MitsubishiMC" };
            var data = new Dictionary<string, object>();

            try
            {
                using (var ms = new MemoryStream(response))
                using (var reader = new BinaryReader(ms))
                {
                    // 跳过Frame Header和Sub Header
                    reader.ReadBytes(24);

                    // 检查错误码
                    var errorCode = reader.ReadUInt16();
                    if (errorCode != 0)
                    {
                        data["Error"] = errorCode;
                        return new DriverParseResult(tags, data);
                    }

                    // 解析数据
                    var mcRequest = MCRequest.Current;
                    if (mcRequest != null && mcRequest.CommandCode == 0x0401) // Read data
                    {
                        var values = new List<ushort>();
                        for (int i = 0; i < mcRequest.Count; i++)
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
                    return Task.FromResult(new DriverHealth(true, "Connected to Mitsubishi MC server"));
                }
                return Task.FromResult(new DriverHealth(false, "Not connected to Mitsubishi MC server"));
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
            _context.Logger.LogInformation("Mitsubishi MC driver disposed");
        }
    }

    /// <summary>
    /// MC协议请求模型
    /// </summary>
    public class MCRequest
    {
        public static MCRequest? Current { get; set; }
        public ushort CommandCode { get; set; } = 0x0401; // Read data registers
        public byte DataType { get; set; } = 0xA8;       // D register
        public ushort StartAddress { get; set; } = 0;
        public ushort Count { get; set; } = 1;
        public byte[]? Data { get; set; }
    }
}