using Host.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MitsubishiMC
{
    /// <summary>
    /// Mitsubishi MC驱动客户端实现
    /// </summary>
    public class MitsubishiMCDriverClient : IDriverClient
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private string _ipAddress = string.Empty;
        private int _port = 5007;
        private byte _unitId = 0;
        private ConnectionState _state = ConnectionState.Disconnected;
        
        // 心跳机制相关字段
        private string _heartbeatAddress = "0";
        private string _heartbeatDataType = "D";

        public ConnectionState State => _state;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public async Task<bool> ConnectAsync(string ipAddress, int port, byte unitId, CancellationToken cancellationToken = default)
        {
            _ipAddress = ipAddress;
            _port = port;
            _unitId = unitId;
            
            ChangeState(ConnectionState.Connecting, "Connecting to Mitsubishi MC server...");

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port, cancellationToken);
                _stream = _client.GetStream();

                ChangeState(ConnectionState.Connected, "Connected to Mitsubishi MC server");
                return true;
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            ChangeState(ConnectionState.Disconnected, "Disconnecting from Mitsubishi MC server...");

            try
            {
                _stream?.Dispose();
                _client?.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                return false;
            }
            finally
            {
                _stream = null;
                _client = null;
                ChangeState(ConnectionState.Disconnected, "Disconnected from Mitsubishi MC server");
            }
        }

        public bool IsConnected
        {
            get { return _client != null && _client.Connected; }
        }

        public async Task<T[]> ReadAsync<T, TRequest>(TRequest request, CancellationToken token = default) where TRequest : ReadRequestBase
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    await ConnectAsync(_ipAddress, _port, _unitId, token);
                }

                // 构建MC协议请求
                var mcRequest = BuildMCRequestForRead(request);

                // 发送请求
                await _stream!.WriteAsync(mcRequest, token);
                await _stream.FlushAsync(token);

                // 接收响应
                var response = await ReadResponseAsync(token);

                // 解析响应
                return ParseMCReadResponse<T>(response, request);
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                return Array.Empty<T>();
            }
        }

        public async Task<bool> WriteAsync<TRequest>(TRequest request, CancellationToken token = default) where TRequest : WriteRequestBase
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    await ConnectAsync(_ipAddress, _port, _unitId, token);
                }

                // 构建MC协议请求
                var mcRequest = BuildMCRequestForWrite(request);

                // 发送请求
                await _stream!.WriteAsync(mcRequest, token);
                await _stream.FlushAsync(token);

                // 接收响应
                var response = await ReadResponseAsync(token);

                // 检查响应是否成功
                return IsResponseSuccessful(response);
            }
            catch (Exception ex)
            {
                ChangeState(ConnectionState.Error, ex.Message);
                return false;
            }
        }

        private byte[] BuildMCRequestForRead<TRequest>(TRequest request) where TRequest : ReadRequestBase
        {
            ushort commandCode = 0x0401; // 读取命令
            byte dataType = 0xA8; // 默认D寄存器

            // 根据功能码确定数据类型
            switch (request.FunctionCode)
            {
                case 1: // 读取线圈
                case 2: // 读取离散输入
                    dataType = 0x90; // M寄存器
                    break;
                case 3: // 读取保持寄存器
                    dataType = 0xA8; // D寄存器
                    break;
                case 4: // 读取输入寄存器
                    dataType = 0x9C; // X寄存器
                    break;
                default:
                    dataType = 0xA8; // 默认D寄存器
                    break;
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
                writer.Write((byte)request.UnitId);  // Unit number
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
                writer.Write((byte)(request.StartingAddress >> 8)); // Start address (high)
                writer.Write((byte)(request.StartingAddress & 0xFF)); // Start address (low)
                writer.Write((ushort)request.Quantity);      // Number of items

                return ms.ToArray();
            }
        }

        private byte[] BuildMCRequestForWrite<TRequest>(TRequest request) where TRequest : WriteRequestBase
        {
            ushort commandCode = 0x1401; // 写入命令
            byte dataType = 0xA8; // 默认D寄存器

            // 根据功能码确定数据类型
            switch (request.FunctionCode)
            {
                case 5: // 写入单个线圈
                case 15: // 写入多个线圈
                    dataType = 0x90; // M寄存器
                    break;
                case 6: // 写入单个保持寄存器
                case 16: // 写入多个保持寄存器
                    dataType = 0xA8; // D寄存器
                    break;
                default:
                    dataType = 0xA8; // 默认D寄存器
                    break;
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
                writer.Write((byte)request.UnitId);  // Unit number
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
                writer.Write((byte)(request.StartingAddress >> 8)); // Start address (high)
                writer.Write((byte)(request.StartingAddress & 0xFF)); // Start address (low)
                writer.Write((ushort)(request.Data.Length / 2)); // Number of items (每个寄存器2字节)

                // 添加写入数据
                writer.Write(request.Data);

                return ms.ToArray();
            }
        }

        private T[] ParseMCReadResponse<T>(byte[] response, ReadRequestBase request)
        {
            var values = new List<T>();

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
                        return Array.Empty<T>();
                    }

                    // 读取数据
                    for (int i = 0; i < request.Quantity; i++)
                    {
                        ushort value = reader.ReadUInt16();
                        
                        // 根据目标类型转换值
                        if (typeof(T) == typeof(bool))
                        {
                            values.Add((T)(object)(value != 0));
                        }
                        else if (typeof(T) == typeof(ushort))
                        {
                            values.Add((T)(object)value);
                        }
                        else if (typeof(T) == typeof(short))
                        {
                            values.Add((T)(object)(short)value);
                        }
                        else if (typeof(T) == typeof(int))
                        {
                            values.Add((T)(object)(int)value);
                        }
                        else if (typeof(T) == typeof(uint))
                        {
                            values.Add((T)(object)(uint)value);
                        }
                        else
                        {
                            values.Add((T)(object)value);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 解析失败，返回空数组
            }

            return values.ToArray();
        }

        private bool IsResponseSuccessful(byte[] response)
        {
            try
            {
                using (var ms = new MemoryStream(response))
                using (var reader = new BinaryReader(ms))
                {
                    // 跳过Frame Header和Sub Header
                    reader.ReadBytes(24);

                    // 检查错误码
                    var errorCode = reader.ReadUInt16();
                    return errorCode == 0;
                }
            }
            catch (Exception)
            {
                return false;
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
                ConnectionId = "", // Mitsubishi MC驱动不使用连接ID
                State = newState
            });
        }
        
        // 保留OpenAsync方法以保持兼容性
        private async Task OpenAsync(CancellationToken token = default)
        {
            await ConnectAsync(_ipAddress, _port, _unitId, token);
        }

        // 心跳机制相关实现
        public bool SupportsNativeHeartbeat => false;
        
        public void SetHeartbeatPoint(string heartbeatAddress, string dataType)
        {
            _heartbeatAddress = heartbeatAddress;
            _heartbeatDataType = dataType;
        }
        
        public async Task<bool> CheckHeartbeatAsync(CancellationToken token = default)
        {
            try
            {
                // 使用ReadAsync方法实现心跳检查
                var request = new ReadCoilRequest
                {
                    UnitId = _unitId,
                    StartingAddress = ushort.Parse(_heartbeatAddress),
                    Quantity = 1
                };
                
                // 执行心跳检查
                var response = await ReadAsync<bool, ReadCoilRequest>(request, token);
                return response.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}