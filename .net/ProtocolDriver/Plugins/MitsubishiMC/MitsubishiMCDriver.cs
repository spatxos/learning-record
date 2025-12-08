using Host.SDK;
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
        public PluginMetadata Metadata => new PluginMetadata("MitsubishiMC", "1.0.0");

        public Task<IProtocolConnection> CreateConnectionAsync(IDictionary<string, string> settings, CancellationToken token = default)
        {
            var connectionId = Guid.NewGuid().ToString();
            var connection = new MitsubishiMCConnection(connectionId, settings);
            return Task.FromResult<IProtocolConnection>(connection);
        }
    }

    public class MitsubishiMCConnection : IProtocolConnection
    {
        private readonly string _connectionId;
        private readonly IDictionary<string, string> _settings;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private ConnectionState _state = ConnectionState.Disconnected;

        public string ConnectionId => _connectionId;
        public IDictionary<string, string> Settings => _settings;
        public ConnectionState State => _state;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        private readonly string _ipAddress;
        private readonly int _port;
        private readonly byte _unitId;

        public MitsubishiMCConnection(string connectionId, IDictionary<string, string> settings)
        {
            _connectionId = connectionId;
            _settings = settings;

            // 从设置中读取连接信息
            _ipAddress = settings.TryGetValue("Host", out var ip) ? ip : "127.0.0.1";
            _port = settings.TryGetValue("Port", out var portStr) && int.TryParse(portStr, out var port) ? port : 5007;
            _unitId = settings.TryGetValue("UnitId", out var unitIdStr) && byte.TryParse(unitIdStr, out var unitId) ? unitId : (byte)0;
        }

        public async Task OpenAsync(CancellationToken token = default)
        {
            ChangeState(ConnectionState.Connecting, "Connecting to Mitsubishi MC server...");

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port, token);
                _stream = _client.GetStream();

                ChangeState(ConnectionState.Connected, "Connected to Mitsubishi MC server");
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                throw;
            }
        }

        public async Task CloseAsync(CancellationToken token = default)
        {
            ChangeState(ConnectionState.Disconnected, "Disconnecting from Mitsubishi MC server...");

            try
            {
                _stream?.Dispose();
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                throw;
            }
            finally
            {
                _stream = null;
                _client = null;
                ChangeState(ConnectionState.Disconnected, "Disconnected from Mitsubishi MC server");
            }
        }

        public async Task<ProtocolResponse> ExecuteAsync(ProtocolRequest request, CancellationToken token = default)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    await OpenAsync(token);
                }

                // 根据请求动作构建MC协议请求
                byte[] mcRequest = BuildMCRequest(request);

                // 发送请求
                await _stream!.WriteAsync(mcRequest, token);
                await _stream.FlushAsync(token);

                // 接收响应
                var response = await ReadResponseAsync(token);

                // 解析响应
                var parsedResponse = ParseMCResponse(response, request);

                return new ProtocolResponse(true, response, parsedResponse);
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                return new ProtocolResponse(false, null, null, ex.Message);
            }
        }

        private byte[] BuildMCRequest(ProtocolRequest request)
        {
            // 解析请求参数
            ushort commandCode = 0x0401; // 默认读取命令
            byte dataType = 0xA8; // 默认D寄存器
            ushort startAddress = 0;
            ushort count = 1;

            if (request.Action.ToLower() == "write")
            {
                commandCode = 0x1401; // 写入命令
            }

            if (request.Props.TryGetValue("DataType", out var dataTypeStr))
            {
                // 根据数据类型字符串转换为MC协议数据类型
                switch (dataTypeStr.ToUpper())
                {
                    case "D": dataType = 0xA8; break;
                    case "M": dataType = 0x90; break;
                    case "X": dataType = 0x9C; break;
                    case "Y": dataType = 0x9D; break;
                    default: dataType = 0xA8; break;
                }
            }

            if (request.Props.TryGetValue("Address", out var addressStr) && ushort.TryParse(addressStr, out startAddress))
            {
                // 地址已解析
            }

            if (request.Props.TryGetValue("Count", out var countStr) && ushort.TryParse(countStr, out count))
            {
                // 数量已解析
            }

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
                writer.Write((ushort)commandCode); // Command code
                writer.Write((ushort)0x0000);     // Subcommand code
                writer.Write((byte)0x00);         // Timer

                // Parameter
                writer.Write((byte)dataType);     // Data type
                writer.Write((ushort)startAddress >> 8); // Start address (high)
                writer.Write((ushort)startAddress & 0xFF); // Start address (low)
                writer.Write((ushort)count);      // Number of items

                // 如果是写入命令，添加数据
                if (commandCode == 0x1401 && request.Payload != null)
                {
                    writer.Write(request.Payload);
                }

                return ms.ToArray();
            }
        }

        private async Task<byte[]> ReadResponseAsync(CancellationToken token)
        {
            // 先读取响应头
            var header = new byte[24];
            await ReadExactAsync(_stream!, header, token);

            // 解析帧长度（从响应头中获取）
            int dataLength = header.Length; // 简化实现，实际应从响应头计算数据长度

            // 读取响应数据
            var response = new List<byte>(header);
            if (dataLength > header.Length)
            {
                var data = new byte[dataLength - header.Length];
                await ReadExactAsync(_stream!, data, token);
                response.AddRange(data);
            }

            return response.ToArray();
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

        private IDictionary<string, object> ParseMCResponse(byte[] response, ProtocolRequest request)
        {
            var parsed = new Dictionary<string, object>();

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
                        parsed["Error"] = errorCode;
                        return parsed;
                    }

                    // 解析数据
                    if (request.Action.ToLower() == "read")
                    {
                        var count = request.Props.TryGetValue("Count", out var countStr) && ushort.TryParse(countStr, out var c) ? c : (ushort)1;
                        var values = new List<ushort>();
                        
                        for (int i = 0; i < count; i++)
                        {
                            values.Add(reader.ReadUInt16());
                        }
                        
                        parsed["Values"] = values;
                    }
                }
            }
            catch (Exception ex)
            {
                parsed["Error"] = ex.Message;
            }

            return parsed;
        }

        private void ChangeState(ConnectionState newState, string message)
        {
            _state = newState;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs
            {
                ConnectionId = _connectionId,
                State = newState,
                Message = message
            });
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}