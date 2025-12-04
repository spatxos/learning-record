using Host.SDK;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OmronFINS
{
    public class OmronFINSDriver : IProtocolDriver
    {
        private DriverContext _context = null!;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private string _ipAddress = "127.0.0.1";
        private int _port = 9600;
        private byte[] _localNode = { 0x00, 0x00, 0x01 };
        private byte[] _remoteNode = { 0x00, 0x00, 0x01 };
        private ushort _sequenceNumber = 0;

        public string ProtocolName => "OmronFINS";
        public string Version => "1.0.0";

        public async Task InitializeAsync(DriverContext context, CancellationToken token = default)
        {
            _context = context;
            _context.Logger.LogInformation("Omron FINS driver initialized");

            // 从配置读取连接信息
            _ipAddress = _context.Config.GetValue<string>("OmronFINS:IpAddress") ?? _ipAddress;
            _port = _context.Config.GetValue<int>("OmronFINS:Port") ?? _port;

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
                _context.Logger.LogInformation("Connected to Omron FINS server: {IpAddress}:{Port}", _ipAddress, _port);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to connect to Omron FINS server: {IpAddress}:{Port}", _ipAddress, _port);
                throw;
            }
        }

        public byte[] BuildRequest(object requestModel)
        {
            if (requestModel is not FINSRequest request)
                throw new ArgumentException("Invalid request model type", nameof(requestModel));

            _context.Logger.LogDebug("BuildRequest: {Command}:{Subcommand} - {AreaCode}{Address}:{Count}", 
                request.Command, request.Subcommand, request.AreaCode, request.Address, request.Count);

            // 保存当前请求，用于解析响应
            FINSRequest.Current = request;

            // 构建FINS协议请求帧
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // FINS TCP头部
                writer.Write((byte)0x46); // 'F'
                writer.Write((byte)0x49); // 'I'
                writer.Write((byte)0x4E); // 'N'
                writer.Write((byte)0x53); // 'S'
                writer.Write((ushort)0x0000); // Reserved
                writer.Write((ushort)0x0000); // Reserved

                // FINS头部
                writer.Write((byte)0x00); // ICFL
                writer.Write((byte)0x00); // RCFL
                writer.Write((byte)0x02); // ICFT
                writer.Write((byte)0x00); // RCFT
                writer.Write((byte)0x00); // GM

                // 本地节点
                writer.Write(_localNode[0]); // Network address
                writer.Write(_localNode[1]); // Node number
                writer.Write(_localNode[2]); // Unit number

                // 远程节点
                writer.Write(_remoteNode[0]); // Network address
                writer.Write(_remoteNode[1]); // Node number
                writer.Write(_remoteNode[2]); // Unit number

                // 服务ID
                writer.Write(_sequenceNumber++);

                // 命令和子命令
                writer.Write((byte)request.Command);
                writer.Write((byte)request.Subcommand);

                // 参数
                writer.Write((byte)request.AreaCode); // Memory area code
                writer.Write((ushort)(request.Address >> 8)); // Start address (high)
                writer.Write((ushort)(request.Address & 0xFF)); // Start address (low)
                writer.Write((byte)0x00); // Bit address (for bit data)
                writer.Write((ushort)(request.Count >> 8)); // Number of items (high)
                writer.Write((ushort)(request.Count & 0xFF)); // Number of items (low)

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
                _context.Logger.LogDebug("Sent FINS request: {Request}", BitConverter.ToString(request));

                // 接收响应
                var response = await ReadResponseAsync(token);
                _context.Logger.LogDebug("Received FINS response: {Response}", BitConverter.ToString(response));

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
            // 先读取FINS TCP头部
            var header = new byte[12];
            await ReadExactAsync(_stream!, header, token);

            // 读取FINS头部
            var finsHeader = new byte[10];
            await ReadExactAsync(_stream!, finsHeader, token);

            // 解析响应长度
            int responseLength = finsHeader[0] << 8 | finsHeader[1];
            responseLength += 2; // 加上ICFL和RCFL

            // 读取响应数据
            var data = new byte[responseLength];
            await ReadExactAsync(_stream!, data, token);

            // 合并所有部分
            var fullResponse = new List<byte>(header);
            fullResponse.AddRange(finsHeader);
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
            // 解析FINS协议响应
            var tags = new string[] { "OmronFINS" };
            var data = new Dictionary<string, object>();

            try
            {
                using (var ms = new MemoryStream(response))
                using (var reader = new BinaryReader(ms))
                {
                    // 跳过FINS TCP头部和FINS头部
                    reader.ReadBytes(22);

                    // 检查响应码
                    var mainResponse = reader.ReadByte();
                    var subResponse = reader.ReadByte();
                    if (mainResponse != 0 || subResponse != 0)
                    {
                        data["Error"] = $"Main: {mainResponse}, Sub: {subResponse}";
                        return new DriverParseResult(tags, data);
                    }

                    // 解析数据
                    var finsRequest = FINSRequest.Current;
                    if (finsRequest != null)
                    {
                        var values = new List<ushort>();
                        for (int i = 0; i < finsRequest.Count; i++)
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
                    return Task.FromResult(new DriverHealth(true, "Connected to Omron FINS server"));
                }
                return Task.FromResult(new DriverHealth(false, "Not connected to Omron FINS server"));
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
            _context.Logger.LogInformation("Omron FINS driver disposed");
        }
    }

    /// <summary>
    /// FINS协议请求模型
    /// </summary>
    public class FINSRequest
    {
        public static FINSRequest? Current { get; set; }
        public byte Command { get; set; } = 0x01; // Memory area read
        public byte Subcommand { get; set; } = 0x01; // Word data
        public byte AreaCode { get; set; } = 0xB0; // DM area
        public uint Address { get; set; } = 0;
        public ushort Count { get; set; } = 1;
    }
}