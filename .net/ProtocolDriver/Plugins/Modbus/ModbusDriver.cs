using Host.SDK;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus
{
    public class ModbusDriver : IProtocolDriver
    {
        public PluginMetadata Metadata => new PluginMetadata("Modbus", "1.0.0");

        public async Task<IProtocolConnection> CreateConnectionAsync(IDictionary<string, string> settings, CancellationToken token = default)
        {
            var connection = new ModbusProtocolConnection(settings);
            await connection.OpenAsync(token);
            return connection;
        }
    }

    internal class ModbusProtocolConnection : IProtocolConnection
    {
        private readonly IDictionary<string, string> _settings;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private string _ipAddress = "127.0.0.1";
        private int _port = 502;
        private byte _unitId = 1;
        private ushort _transactionId = 0; // 事务标识符
        
        // 心跳机制相关字段
        private string _heartbeatAddress = "0"; // 默认心跳检查地址
        private string _heartbeatDataType = "Coil"; // 默认心跳检查数据类型

        public string ConnectionId { get; } = Guid.NewGuid().ToString();
        public IDictionary<string, string> Settings => _settings;
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public ModbusProtocolConnection(IDictionary<string, string> settings)
        {
            _settings = settings;
            
            // 从配置读取连接信息
            // 兼容Host和IpAddress两种键名
            if (_settings.TryGetValue("Host", out var host)) _ipAddress = host;
            else if (_settings.TryGetValue("IpAddress", out var ip)) _ipAddress = ip;
            
            if (_settings.TryGetValue("Port", out var portStr) && int.TryParse(portStr, out var port)) _port = port;
            if (_settings.TryGetValue("UnitId", out var unitIdStr) && byte.TryParse(unitIdStr, out var unitId)) _unitId = unitId;
        }

        public async Task OpenAsync(CancellationToken token = default)
        {
            if (State == ConnectionState.Connected)
                return;

            UpdateState(ConnectionState.Connecting, "Connecting...");
            
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, _port, token);
                _networkStream = _tcpClient.GetStream();
                
                UpdateState(ConnectionState.Connected, "Connected");
            } 
            catch (Exception ex)
            {
                UpdateState(ConnectionState.Error, ex.Message);
                throw;
            }
        }

        public async Task CloseAsync(CancellationToken token = default)
        {
            if (State == ConnectionState.Disconnected)
                return;

            try
            {
                _networkStream?.Close();
                _tcpClient?.Close();
            } 
            finally
            {
                UpdateState(ConnectionState.Disconnected, "Disconnected");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _networkStream?.Dispose();
                _tcpClient?.Dispose();
            }
        }
        
        #region 心跳机制实现
        
        /// <summary>
        /// Modbus协议本身不支持原生心跳，我们将使用用户指定的地址位作为心跳检查点位
        /// </summary>
        public bool SupportsNativeHeartbeat => false;
        
        /// <summary>
        /// 设置心跳检查点位
        /// </summary>
        /// <param name="heartbeatAddress">心跳检查的地址</param>
        /// <param name="dataType">数据类型</param>
        public void SetHeartbeatPoint(string heartbeatAddress, string dataType)
        {
            _heartbeatAddress = heartbeatAddress;
            _heartbeatDataType = dataType;
        }
        
        /// <summary>
        /// 执行心跳检查
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>心跳是否成功</returns>
        public async Task<bool> CheckHeartbeatAsync(CancellationToken token = default)
        {
            if (State != ConnectionState.Connected)
                return false;
            
            try
            {
                // 构建心跳检查请求
                var request = new ProtocolRequest("read", new Dictionary<string, string>
                {
                    { "Address", _heartbeatAddress },
                    { "DataType", _heartbeatDataType },
                    { "Count", "1" }
                });
                
                // 执行心跳检查请求，使用较短的超时时间
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(2000); // 心跳检查超时时间为2秒
                
                var response = await ExecuteAsync(request, cts.Token);
                return response.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        #endregion

        public async Task<ProtocolResponse> ExecuteAsync(ProtocolRequest request, CancellationToken token = default)
        {
            if (State != ConnectionState.Connected)
                throw new InvalidOperationException("Connection is not open");

            try
            {
                // 解析请求参数
                string action = request.Action;
                IDictionary<string, string> props = request.Props;

                // 根据动作构建请求
                ModbusRequest modbusRequest = BuildModbusRequest(action, props);

                // 构建Modbus TCP请求帧
                byte[] modbusFrame = BuildModbusTcpFrame((byte)modbusRequest.FunctionCode, modbusRequest);

                // 发送请求
                Console.WriteLine($"DEBUG: Sending request frame: {BitConverter.ToString(modbusFrame)}");
                await _networkStream!.WriteAsync(modbusFrame, 0, modbusFrame.Length, token);
                await _networkStream.FlushAsync(token);

                // 接收响应
                byte[] responseBuffer = new byte[1024];
                int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);

                // 增加调试信息
                Console.WriteLine($"DEBUG: Total bytes read = {bytesRead}");
                Console.WriteLine($"DEBUG: Response buffer (first {Math.Min(bytesRead, 20)} bytes): {BitConverter.ToString(responseBuffer, 0, Math.Min(bytesRead, 20))}");

                if (bytesRead < 9) // 最小Modbus TCP响应长度
                {
                    return new ProtocolResponse(false, null, null, $"Invalid Modbus TCP response received. Expected at least 9 bytes, got {bytesRead}");
                }

                // 提取Modbus TCP帧头信息
                ushort transactionId = (ushort)((responseBuffer[0] << 8) | responseBuffer[1]);
                ushort protocolId = (ushort)((responseBuffer[2] << 8) | responseBuffer[3]);
                ushort length = (ushort)((responseBuffer[4] << 8) | responseBuffer[5]);
                byte unitId = responseBuffer[6];

                // 调试帧头信息
                Console.WriteLine($"DEBUG: Transaction ID = {transactionId} (0x{transactionId:X4})");
                Console.WriteLine($"DEBUG: Protocol ID = {protocolId} (0x{protocolId:X4})");
                Console.WriteLine($"DEBUG: Length = {length} (0x{length:X4})");
                Console.WriteLine($"DEBUG: Unit ID = {unitId} (0x{unitId:X2})");

                // 提取Modbus PDU
                int expectedPduLength = length - 1; // 减去Unit ID的长度
                int actualPduLengthAvailable = bytesRead - 7; // 响应总长度减去帧头7字节
                
                Console.WriteLine($"DEBUG: Expected PDU length = {expectedPduLength}");
                Console.WriteLine($"DEBUG: Actual PDU bytes available = {actualPduLengthAvailable}");

                int pduLength = Math.Min(expectedPduLength, actualPduLengthAvailable);
                byte[] pdu = new byte[pduLength];
                Array.Copy(responseBuffer, 7, pdu, 0, pduLength); // PDU从索引7开始（Transaction ID(2) + Protocol ID(2) + Length(2) + Unit ID(1) = 7）

                byte[] responseBytes = Array.Empty<byte>();
                IDictionary<string, object>? parsedData = null;

                // 调试信息
                Console.WriteLine($"DEBUG: Request FunctionCode = {modbusRequest.FunctionCode} (0x{modbusRequest.FunctionCode:X2})");
                Console.WriteLine($"DEBUG: Response PDU Length = {pdu.Length}");
                Console.WriteLine($"DEBUG: Response PDU[0] = {pdu[0]} (0x{pdu[0]:X2})");
                Console.WriteLine($"DEBUG: Full response buffer: {BitConverter.ToString(responseBuffer, 0, bytesRead)}");

                // 处理响应
                Console.WriteLine($"DEBUG: Full request frame: {BitConverter.ToString(modbusFrame)}");
                Console.WriteLine($"DEBUG: Request function code: {(byte)modbusRequest.FunctionCode:X2}");
                Console.WriteLine($"DEBUG: Response PDU: {BitConverter.ToString(pdu)}");
                Console.WriteLine($"DEBUG: Response PDU[0]: {pdu[0]:X2}");
                
                if (pdu[0] == (byte)modbusRequest.FunctionCode)
                {
                    switch ((ModbusFunctionCode)modbusRequest.FunctionCode)
                    {
                        case ModbusFunctionCode.ReadCoils:
                        case ModbusFunctionCode.ReadDiscreteInputs:
                            // 提取线圈/离散输入数据
                            responseBytes = new byte[pdu[1]];
                            Array.Copy(pdu, 2, responseBytes, 0, pdu[1]);
                            parsedData = new Dictionary<string, object> { { "Coils", responseBytes.Select(b => b != 0).ToList() } };
                            break;
                        case ModbusFunctionCode.ReadHoldingRegisters:
                        case ModbusFunctionCode.ReadInputRegisters:
                            // 提取寄存器数据 (big-endian格式)
                            responseBytes = new byte[pdu[1]];
                            Array.Copy(pdu, 2, responseBytes, 0, pdu[1]);
                            
                            var registers = new List<ushort>();
                            for (int i = 0; i < responseBytes.Length; i += 2)
                            {
                                if (i + 1 < responseBytes.Length)
                                {
                                    ushort value = (ushort)((responseBytes[i] << 8) | responseBytes[i + 1]);
                                    registers.Add(value);
                                }
                            }
                            parsedData = new Dictionary<string, object> { { "Registers", registers } };
                            break;
                        case ModbusFunctionCode.WriteSingleCoil:
                            // 返回线圈地址和值
                            responseBytes = new byte[4];
                            Array.Copy(pdu, 1, responseBytes, 0, 4);
                            parsedData = new Dictionary<string, object> { { "CoilValue", responseBytes[3] != 0 } };
                            break;
                        case ModbusFunctionCode.WriteSingleRegister:
                            // 返回寄存器地址和值 (big-endian格式)
                            responseBytes = new byte[4];
                            Array.Copy(pdu, 1, responseBytes, 0, 4);
                            ushort registerValue = (ushort)((responseBytes[2] << 8) | responseBytes[3]);
                            parsedData = new Dictionary<string, object> { { "RegisterValue", registerValue } };
                            break;
                        default:
                            return new ProtocolResponse(false, null, null, $"Function code not implemented: {modbusRequest.FunctionCode}");
                    }

                    return new ProtocolResponse(true, responseBytes, parsedData);
                } 
                else if (pdu[0] == (byte)(modbusRequest.FunctionCode | 0x80))
                {
                    // 处理错误响应
                    return new ProtocolResponse(false, null, null, $"Modbus error: {pdu[1]}");
                } 
                else
                {
                    return new ProtocolResponse(false, null, null, "Unexpected Modbus function code in response");
                }
            } 
            catch (Exception ex)
            {
                UpdateState(ConnectionState.Error, ex.Message);
                return new ProtocolResponse(false, null, null, ex.Message);
            }
        }

        private void UpdateState(ConnectionState newState, string message)
        {
            State = newState;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs
            {
                ConnectionId = ConnectionId,
                State = newState,
                Message = message
            });
        }

        private ModbusRequest BuildModbusRequest(string action, IDictionary<string, string> props)
        {
            ModbusRequest request = new ModbusRequest();

            // 根据动作确定功能码
            action = action.ToLower();
            
            // 处理通用的read/write动作
            if (action == "read")
            {
                // 根据DataType确定具体的read功能
                if (props.TryGetValue("DataType", out var dataType))
                {
                    dataType = dataType.ToLower();
                    switch (dataType)
                    {
                        case "bool":
                            // 默认为读线圈
                            request.FunctionCode = (int)ModbusFunctionCode.ReadCoils;
                            break;
                        default:
                            // 默认为读保持寄存器
                            request.FunctionCode = (int)ModbusFunctionCode.ReadHoldingRegisters;
                            break;
                    }
                }
                else
                {
                    // 默认读保持寄存器
                    request.FunctionCode = (int)ModbusFunctionCode.ReadHoldingRegisters;
                }
            }
            else if (action == "write")
            {
                // 根据DataType确定具体的write功能
                if (props.TryGetValue("DataType", out var dataType))
                {
                    dataType = dataType.ToLower();
                    switch (dataType)
                    {
                        case "bool":
                            // 写单个线圈
                            request.FunctionCode = (int)ModbusFunctionCode.WriteSingleCoil;
                            break;
                        default:
                            // 写单个寄存器
                            request.FunctionCode = (int)ModbusFunctionCode.WriteSingleRegister;
                            break;
                    }
                }
                else
                {
                    // 默认写单个寄存器
                    request.FunctionCode = (int)ModbusFunctionCode.WriteSingleRegister;
                }
            }
            // 处理具体的Modbus功能动作
            else
            {
                switch (action)
                {
                    case "readcoils":
                        request.FunctionCode = (int)ModbusFunctionCode.ReadCoils;
                        break;
                    case "readdiscreteinputs":
                        request.FunctionCode = (int)ModbusFunctionCode.ReadDiscreteInputs;
                        break;
                    case "readholdingregisters":
                        request.FunctionCode = (int)ModbusFunctionCode.ReadHoldingRegisters;
                        break;
                    case "readinputregisters":
                        request.FunctionCode = (int)ModbusFunctionCode.ReadInputRegisters;
                        break;
                    case "writesinglecoil":
                        request.FunctionCode = (int)ModbusFunctionCode.WriteSingleCoil;
                        break;
                    case "writesingleregister":
                        request.FunctionCode = (int)ModbusFunctionCode.WriteSingleRegister;
                        break;
                    default:
                        throw new NotImplementedException($"Action not implemented: {action}");
                }
            }

            // 解析公共参数
            if (props.TryGetValue("Address", out var startAddressStr) && ushort.TryParse(startAddressStr, out var startAddress))
            {
                request.StartAddress = startAddress;
            }
            else if (props.TryGetValue("Address", out var addressStr) && ushort.TryParse(addressStr, out var address))
            {
                // 兼容Address参数
                request.StartAddress = address;
            }

            if (props.TryGetValue("Count", out var countStr) && ushort.TryParse(countStr, out var count))
            {
                request.Count = count;
            }
            else
            {
                // 默认读取1个寄存器
                request.Count = 1;
            }

            // 解析写入操作的数据
            if (request.FunctionCode == (int)ModbusFunctionCode.WriteSingleCoil)
            {
                bool value = props.TryGetValue("Value", out var valueStr) && bool.Parse(valueStr);
                request.Data = new byte[] { (byte)(value ? 0xFF : 0x00), 0x00 };
            }
            else if (request.FunctionCode == (int)ModbusFunctionCode.WriteSingleRegister)
            {
                if (props.TryGetValue("Value", out var valueStr) && ushort.TryParse(valueStr, out var value))
                {
                    request.Data = new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
                }
            }

            return request;
        }

        /// <summary>
        /// 构建Modbus TCP请求帧
        /// </summary>
        private byte[] BuildModbusTcpFrame(byte functionCode, ModbusRequest modbusRequest)
        {
            // Modbus TCP帧格式：
            // 事务标识符 (2字节)
            // 协议标识符 (2字节) - 0表示Modbus TCP
            // 长度 (2字节) - 后续字节数
            // 单元标识符 (1字节)
            // PDU (功能码 + 数据)

            byte[] pdu;
            switch ((ModbusFunctionCode)functionCode)
            {
                case ModbusFunctionCode.ReadCoils:
                case ModbusFunctionCode.ReadDiscreteInputs:
                case ModbusFunctionCode.ReadHoldingRegisters:
                case ModbusFunctionCode.ReadInputRegisters:
                    // PDU: 功能码(1) + 起始地址(2) + 数量(2)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8); // 起始地址高字节
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF); // 起始地址低字节
                    pdu[3] = (byte)(modbusRequest.Count >> 8); // 数量高字节
                    pdu[4] = (byte)(modbusRequest.Count & 0xFF); // 数量低字节
                    break;
                case ModbusFunctionCode.WriteSingleCoil:
                    // PDU: 功能码(1) + 地址(2) + 值(2)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    bool coilValue = modbusRequest.Data != null && modbusRequest.Data.Length > 0 && modbusRequest.Data[0] != 0;
                    pdu[3] = coilValue ? (byte)0xFF : (byte)0x00;
                    pdu[4] = 0x00;
                    break;
                case ModbusFunctionCode.WriteSingleRegister:
                    // PDU: 功能码(1) + 地址(2) + 值(2, big-endian)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    if (modbusRequest.Data != null && modbusRequest.Data.Length >= 2)
                    {
                        // 使用big-endian格式
                        pdu[3] = modbusRequest.Data[0];
                        pdu[4] = modbusRequest.Data[1];
                    }
                    else
                    {
                        pdu[3] = 0x00;
                        pdu[4] = 0x00;
                    }
                    break;
                default:
                    throw new NotImplementedException($"Function code not implemented: {functionCode}");
            }

            // 构建完整的Modbus TCP帧
            byte[] frame = new byte[7 + pdu.Length];
            frame[0] = (byte)(_transactionId >> 8); // 事务标识符高字节
            frame[1] = (byte)(_transactionId & 0xFF); // 事务标识符低字节
            _transactionId++; // 事务标识符递增
            frame[2] = 0x00; // 协议标识符高字节
            frame[3] = 0x00; // 协议标识符低字节
            frame[4] = (byte)((pdu.Length + 1) >> 8); // 长度高字节
            frame[5] = (byte)((pdu.Length + 1) & 0xFF); // 长度低字节
            frame[6] = _unitId; // 单元标识符
            Array.Copy(pdu, 0, frame, 7, pdu.Length);

            return frame;
        }
    }

    /// <summary>
    /// Modbus功能码
    /// </summary>
    public enum ModbusFunctionCode
    {
        ReadCoils = 1,
        ReadDiscreteInputs = 2,
        ReadHoldingRegisters = 3,
        ReadInputRegisters = 4,
        WriteSingleCoil = 5,
        WriteSingleRegister = 6,
        WriteMultipleCoils = 15,
        WriteMultipleRegisters = 16
    }

    /// <summary>
    /// Modbus请求模型
    /// </summary>
    public class ModbusRequest
    {
        public int FunctionCode { get; set; } = (int)ModbusFunctionCode.ReadHoldingRegisters;
        public ushort StartAddress { get; set; } = 0;
        public ushort Count { get; set; } = 1;
        public byte[]? Data { get; set; }
    }
}