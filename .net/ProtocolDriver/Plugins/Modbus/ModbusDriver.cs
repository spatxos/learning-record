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
        private DriverContext _context = null!;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private string _ipAddress = "127.0.0.1";
        private int _port = 502;
        private byte _unitId = 1;

        public string ProtocolName => "Modbus";
        public string Version => "1.0.0";

        public async Task InitializeAsync(DriverContext context, CancellationToken token = default)
        {
            _context = context;
            _context.Logger.LogInformation("Modbus driver initialized");

            // 从配置读取连接信息
            if (_context.Config["Modbus:IpAddress"] != null)
            {
                _ipAddress = _context.Config["Modbus:IpAddress"]!;
            }
            if (_context.Config["Modbus:Port"] != null && int.TryParse(_context.Config["Modbus:Port"], out int port))
            {
                _port = port;
            }
            if (_context.Config["Modbus:UnitId"] != null && byte.TryParse(_context.Config["Modbus:UnitId"], out byte unitId))
            {
                _unitId = unitId;
            }

            // 建立连接
            await ConnectAsync(token);
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            try
            {
                _tcpClient = _context.TransportFactory.CreateTcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, _port, token);
                _networkStream = _tcpClient.GetStream();
                
                _context.Logger.LogInformation("Connected to Modbus server: {IpAddress}:{Port}", _ipAddress, _port);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to connect to Modbus server: {IpAddress}:{Port}", _ipAddress, _port);
                throw;
            }
        }

        public byte[] BuildRequest(object requestModel)
        {
            if (requestModel is not ModbusRequest request)
                throw new ArgumentException("Invalid request model type", nameof(requestModel));

            // 使用NModbus的内部格式构建请求（简化实现）
            // 实际应用中可以直接构造Modbus TCP包
            _context.Logger.LogDebug("BuildRequest: {FunctionCode} - {StartAddress}:{Count}", 
                request.FunctionCode, request.StartAddress, request.Count);

            // 保存当前请求用于后续处理
            ModbusRequest.Current = request;
            
            // 返回功能码字节（1字节）
            return new byte[] { (byte)request.FunctionCode };
        }

        public async Task<DriverResult> ExecuteAsync(byte[] request, CancellationToken token = default)
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    await ConnectAsync(token);
                }

                // 解析请求类型
                var functionCode = request[0]; // 功能码是单字节
                var modbusRequest = ModbusRequest.Current;

                // 构建Modbus TCP请求帧
                byte[] modbusFrame = BuildModbusTcpFrame(functionCode, modbusRequest);

                // 发送请求
                await _networkStream!.WriteAsync(modbusFrame, 0, modbusFrame.Length, token);
                await _networkStream.FlushAsync(token);

                // 接收响应
                byte[] responseBuffer = new byte[1024];
                int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);

                if (bytesRead < 9) // 最小Modbus TCP响应长度
                {
                    throw new InvalidOperationException("Invalid Modbus TCP response received");
                }

                // 提取Modbus PDU
                int dataLength = (responseBuffer[4] << 8) | responseBuffer[5];
                byte[] pdu = new byte[dataLength - 1]; // 减去单位标识符长度
                Array.Copy(responseBuffer, 8, pdu, 0, pdu.Length);

                byte[] responseBytes = Array.Empty<byte>();

                // 处理响应
                if (pdu[0] == functionCode)
                {
                    switch ((ModbusFunctionCode)functionCode)
                    {
                        case ModbusFunctionCode.ReadCoils:
                        case ModbusFunctionCode.ReadDiscreteInputs:
                            // 提取线圈/离散输入数据
                            responseBytes = new byte[pdu[1]];
                            Array.Copy(pdu, 2, responseBytes, 0, pdu[1]);
                            break;
                        case ModbusFunctionCode.ReadHoldingRegisters:
                        case ModbusFunctionCode.ReadInputRegisters:
                            // 提取寄存器数据 (big-endian格式)
                            int registerCount = pdu[1] / 2;
                            responseBytes = new byte[pdu[1]];
                            Array.Copy(pdu, 2, responseBytes, 0, pdu[1]);
                            break;
                        case ModbusFunctionCode.WriteSingleCoil:
                            // 返回线圈地址和值
                            responseBytes = new byte[4];
                            Array.Copy(pdu, 1, responseBytes, 0, 4);
                            break;
                        case ModbusFunctionCode.WriteSingleRegister:
                            // 返回寄存器地址和值 (big-endian格式)
                            responseBytes = new byte[4];
                            Array.Copy(pdu, 1, responseBytes, 0, 4);
                            break;
                        // 可以添加更多功能码支持
                        default:
                            throw new NotImplementedException($"Function code not implemented: {functionCode}");
                    }
                }
                else if (pdu[0] == (byte)(functionCode | 0x80))
                {
                    // 处理错误响应
                    throw new InvalidOperationException($"Modbus error: {pdu[1]}");
                }
                else
                {
                    throw new InvalidOperationException("Unexpected Modbus function code in response");
                }

                _context.Logger.LogDebug("ExecuteAsync completed successfully");
                return new DriverResult(true, responseBytes);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "ExecuteAsync failed");
                return new DriverResult(false, Array.Empty<byte>(), ex.Message);
            }
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
            frame[0] = 0x00; // 事务标识符高字节 (简化处理)
            frame[1] = 0x01; // 事务标识符低字节
            frame[2] = 0x00; // 协议标识符高字节
            frame[3] = 0x00; // 协议标识符低字节
            frame[4] = (byte)((pdu.Length + 1) >> 8); // 长度高字节
            frame[5] = (byte)((pdu.Length + 1) & 0xFF); // 长度低字节
            frame[6] = _unitId; // 单元标识符
            Array.Copy(pdu, 0, frame, 7, pdu.Length);

            return frame;
        }

        public DriverParseResult ParseResponse(byte[] response)
        {
            // 解析Modbus响应
            var tags = new string[] { "Modbus" };
            var data = new Dictionary<string, object>();

            // 根据请求类型解析响应
            var modbusRequest = ModbusRequest.Current;
            if (modbusRequest != null)
            {
                switch ((ModbusFunctionCode)modbusRequest.FunctionCode)
                {
                    case ModbusFunctionCode.ReadCoils:
                        data["Coils"] = response.Select(b => b != 0).ToList();
                        break;
                    case ModbusFunctionCode.ReadHoldingRegisters:
                    case ModbusFunctionCode.ReadInputRegisters:
                        // 将byte数组转换为ushort数组，使用Modbus标准的big-endian格式
                        var registers = new List<ushort>();
                        for (int i = 0; i < response.Length; i += 2)
                        {
                            if (i + 1 < response.Length)
                            {
                                // 标准Modbus使用big-endian格式
                                ushort value = (ushort)((response[i] << 8) | response[i + 1]);
                                registers.Add(value);
                            }
                        }
                        data["Registers"] = registers;
                        break;
                    case ModbusFunctionCode.WriteSingleCoil:
                        data["CoilValue"] = response[3] != 0; // 线圈值在第4字节
                        break;
                    case ModbusFunctionCode.WriteSingleRegister:
                        // 使用Modbus标准的big-endian格式解析
                        ushort registerValue = (ushort)((response[2] << 8) | response[3]);
                        data["RegisterValue"] = registerValue;
                        break;
                }
            }

            return new DriverParseResult(tags, data);
        }

        public Task<DriverHealth> CheckHealthAsync(CancellationToken token = default)
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    return Task.FromResult(new DriverHealth(true, "Connected to Modbus server"));
                }
                return Task.FromResult(new DriverHealth(false, "Not connected to Modbus server"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new DriverHealth(false, ex.Message));
            }
        }

        public void Dispose()
        {
            _networkStream?.Close();
            _tcpClient?.Close();
            _context.Logger.LogInformation("Modbus driver disposed");
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
        public static ModbusRequest? Current { get; set; }
        public int FunctionCode { get; set; }
        public ushort StartAddress { get; set; }
        public ushort Count { get; set; }
        public byte[]? Data { get; set; }
    }
}