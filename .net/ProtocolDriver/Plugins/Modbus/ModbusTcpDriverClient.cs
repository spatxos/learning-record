using Host.SDK;
using Host.SDK.ByteTransform;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net;

namespace Modbus
{
    /// <summary>
    /// Modbus TCP驱动客户端实现
    /// </summary>
    public class ModbusTcpDriverClient : IDeviceCommunication
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private byte _unitId = 1;
        private ushort _transactionId = 0;

        /// <summary>
        /// 字节转换工具
        /// </summary>
        public IByteTransform ByteTransform { get; set; } = new RegularByteTransform();

        /// <summary>
        /// 设备连接唯一标识
        /// </summary>
        public string ConnectionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// 端口号
        /// </summary>
        public string Port { get; set; } = "502";

        /// <summary>
        /// 开始位
        /// </summary>
        public int StartAddress { get; set; } = 0;

        /// <summary>
        /// 单元ID
        /// </summary>
        public byte UnitId
        {
            get => _unitId;
            set => _unitId = value;
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                    return false;

                try
                {
                    var socket = _tcpClient.Client;
                    bool poll = socket.Poll(0, SelectMode.SelectRead);
                    bool available = socket.Available == 0;
                    return !(poll && available);
                }
                catch
                {
                    return false;
                }
            }
        }


        public async Task<bool> ConnectAsync(string host, int port, byte unitId, CancellationToken token = default)
        {
            try
            {
                this.IPAddress = host;
                this.Port = port.ToString();
                this.UnitId = unitId;

                await DisconnectAsync();

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IPAddress, int.Parse(Port), token);
                _networkStream = _tcpClient.GetStream();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 连接到Modbus TCP设备
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                await DisconnectAsync();

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IPAddress, int.Parse(Port));
                _networkStream = _tcpClient.GetStream();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 断开与Modbus TCP设备的连接
        /// </summary>
        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_networkStream != null)
                {
                    await _networkStream.DisposeAsync();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 地址解析结果
        /// </summary>
        private struct AddressParseResult
        {
            public byte FunctionCode;
            public ushort RegisterAddress;
            public int BitIndex;
            public bool IsBitAccess;
        }

        /// <summary>
        /// 解析地址字符串
        /// </summary>
        /// <param name="address">地址字符串，支持格式：100.1, x=4;100.1</param>
        /// <returns>解析结果</returns>
        private AddressParseResult ParseAddress(string address)
        {
            var result = new AddressParseResult
            {
                FunctionCode = 3, // 默认使用功能码3（保持寄存器）
                RegisterAddress = 0,
                BitIndex = 0,
                IsBitAccess = false
            };

            // 解析功能码前缀，如 x=4;100.1
            if (address.Contains(';'))
            {
                var parts = address.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var functionCodePart = parts[0].Trim().ToLower();
                    address = parts[1].Trim();

                    if (functionCodePart.StartsWith("x="))
                    {
                        if (byte.TryParse(functionCodePart.Substring(2), out byte fc))
                        {
                            result.FunctionCode = fc;
                        }
                    }
                }
            }

            // 解析寄存器地址和位索引，如 100.1
            if (address.Contains('.'))
            {
                var parts = address.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    if (ushort.TryParse(parts[0], out ushort regAddr) &&
                        int.TryParse(parts[1], out int bitIndex))
                    {
                        result.RegisterAddress = regAddr;
                        result.BitIndex = bitIndex;
                        result.IsBitAccess = true;
                    }
                }
            }
            else
            {
                // 仅寄存器地址，如 100
                if (ushort.TryParse(address, out ushort regAddr))
                {
                    result.RegisterAddress = regAddr;
                }
            }

            return result;
        }

        /// <summary>
        /// 构建Modbus TCP帧
        /// </summary>
        private byte[] BuildModbusTcpFrame(byte functionCode, ushort startAddress, ushort count, byte[] data = null)
        {
            _transactionId++;

            // 构建PDU
            byte[] pdu;
            if (data == null)
            {
                // 读取请求
                pdu = new byte[5];
                pdu[0] = functionCode;
                pdu[1] = (byte)(startAddress >> 8);
                pdu[2] = (byte)(startAddress & 0xFF);
                pdu[3] = (byte)(count >> 8);
                pdu[4] = (byte)(count & 0xFF);
            }
            else
            {
                // 写入请求
                if (functionCode == 5 || functionCode == 6) // 写入单个线圈或寄存器
                {
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(startAddress >> 8);
                    pdu[2] = (byte)(startAddress & 0xFF);
                    if (data != null && data.Length >= 2)
                    {
                        pdu[3] = data[0];
                        pdu[4] = data[1];
                    }
                }
                else if (functionCode == 15) // 写入多个线圈
                {
                    pdu = new byte[6 + data.Length];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(startAddress >> 8);
                    pdu[2] = (byte)(startAddress & 0xFF);
                    pdu[3] = (byte)(count >> 8);
                    pdu[4] = (byte)(count & 0xFF);
                    pdu[5] = (byte)data.Length;
                    Array.Copy(data, 0, pdu, 6, data.Length);
                }
                else // 写入多个寄存器
                {
                    pdu = new byte[6 + data.Length];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(startAddress >> 8);
                    pdu[2] = (byte)(startAddress & 0xFF);
                    pdu[3] = (byte)(count >> 8);
                    pdu[4] = (byte)(count & 0xFF);
                    pdu[5] = (byte)data.Length;
                    Array.Copy(data, 0, pdu, 6, data.Length);
                }
            }

            // 构建MBAP头
            byte[] mbap = new byte[7];
            mbap[0] = (byte)(_transactionId >> 8);
            mbap[1] = (byte)(_transactionId & 0xFF);
            mbap[2] = 0x00; // 协议标识符高字节
            mbap[3] = 0x00; // 协议标识符低字节
            mbap[4] = (byte)((pdu.Length + 1) >> 8); // 长度高字节
            mbap[5] = (byte)((pdu.Length + 1) & 0xFF); // 长度低字节
            mbap[6] = _unitId; // 单元标识符

            // 组合帧
            byte[] frame = new byte[mbap.Length + pdu.Length];
            Array.Copy(mbap, 0, frame, 0, mbap.Length);
            Array.Copy(pdu, 0, frame, mbap.Length, pdu.Length);

            return frame;
        }

        /// <summary>
        /// 发送请求并接收响应
        /// </summary>
        private async Task<byte[]> SendRequestAsync(byte[] requestFrame, CancellationToken token = default)
        {
            if (_tcpClient == null || !_tcpClient.Connected || _networkStream == null)
            {
                throw new InvalidOperationException("Not connected to Modbus TCP server");
            }

            // 发送请求
            await _networkStream.WriteAsync(requestFrame, 0, requestFrame.Length, token);
            await _networkStream.FlushAsync(token);

            // 接收响应
            byte[] responseBuffer = new byte[1024];
            int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);

            if (bytesRead < 9)
            {
                throw new InvalidOperationException("Invalid response from Modbus TCP server");
            }

            // 提取数据部分
            int dataLength = (responseBuffer[4] << 8) | responseBuffer[5];
            byte[] responseData = new byte[dataLength - 1];
            Array.Copy(responseBuffer, 7, responseData, 0, responseData.Length);

            // 检查错误响应
            if ((responseData[0] & 0x80) != 0)
            {
                throw new Exception($"Modbus error: {responseData[1]}");
            }

            return responseData;
        }

        /// <summary>
        /// 读取单个寄存器值
        /// </summary>
        private async Task<ushort> ReadSingleRegisterAsync(ushort address, byte functionCode, CancellationToken token = default)
        {
            byte[] requestFrame = BuildModbusTcpFrame(functionCode, address, 1);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应格式: 功能码(1) + 字节数(1) + 数据(n)
            if (response.Length < 3)
            {
                throw new InvalidOperationException("Invalid response from Modbus TCP server");
            }

            return (ushort)(response[2] << 8 | response[3]);
        }

        /// <summary>
        /// 读取多个寄存器值
        /// </summary>
        private async Task<ushort[]> ReadMultipleRegistersAsync(ushort address, ushort count, byte functionCode, CancellationToken token = default)
        {
            byte[] requestFrame = BuildModbusTcpFrame(functionCode, address, count);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应格式: 功能码(1) + 字节数(1) + 数据(n)
            if (response.Length < 3 || response[1] != count * 2)
            {
                throw new InvalidOperationException("Invalid response from Modbus TCP server");
            }

            ushort[] registers = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                registers[i] = (ushort)(response[2 + i * 2] << 8 | response[3 + i * 2]);
            }

            return registers;
        }

        /// <summary>
        /// 写入单个寄存器
        /// </summary>
        private async Task<bool> WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken token = default)
        {
            byte[] data = new byte[2];
            data[0] = (byte)(value >> 8);
            data[1] = (byte)(value & 0xFF);

            byte[] requestFrame = BuildModbusTcpFrame(6, address, 1, data);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应应该与请求相同
            return response.Length == 5 && 
                   response[0] == 6 && 
                   (ushort)(response[1] << 8 | response[2]) == address &&
                   (ushort)(response[3] << 8 | response[4]) == value;
        }

        /// <summary>
        /// 写入多个寄存器
        /// </summary>
        private async Task<bool> WriteMultipleRegistersAsync(ushort address, ushort[] values, CancellationToken token = default)
        {
            byte[] data = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                data[i * 2] = (byte)(values[i] >> 8);
                data[i * 2 + 1] = (byte)(values[i] & 0xFF);
            }

            byte[] requestFrame = BuildModbusTcpFrame(16, address, (ushort)values.Length, data);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应格式: 功能码(1) + 起始地址(2) + 数量(2)
            return response.Length == 5 && 
                   response[0] == 16 && 
                   (ushort)(response[1] << 8 | response[2]) == address &&
                   (ushort)(response[3] << 8 | response[4]) == values.Length;
        }

        /// <summary>
        /// 读取单个线圈值
        /// </summary>
        private async Task<bool> ReadSingleCoilAsync(ushort address, CancellationToken token = default)
        {
            // 读取线圈使用功能码1
            byte[] requestFrame = BuildModbusTcpFrame(1, address, 1);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应格式: 功能码(1) + 字节数(1) + 数据(1)
            if (response.Length < 3)
            {
                throw new InvalidOperationException("Invalid response from Modbus TCP server");
            }

            return response[2] != 0;
        }

        /// <summary>
        /// 写入单个线圈值
        /// </summary>
        private async Task<bool> WriteSingleCoilAsync(ushort address, bool value, CancellationToken token = default)
        {
            // 写入线圈使用功能码5
            byte[] data = new byte[2];
            if (value)
            {
                data[0] = 0xFF;
                data[1] = 0x00;
            }
            else
            {
                data[0] = 0x00;
                data[1] = 0x00;
            }

            byte[] requestFrame = BuildModbusTcpFrame(5, address, 1, data);
            byte[] response = await SendRequestAsync(requestFrame, token);

            // 响应应该与请求相同
            return response.Length == 5 && 
                   response[0] == 5 && 
                   (ushort)(response[1] << 8 | response[2]) == address &&
                   ((value && response[3] == 0xFF && response[4] == 0x00) || 
                    (!value && response[3] == 0x00 && response[4] == 0x00));
        }

        #region IReadWriteNet 接口实现

        public OperateResult<byte[]> Read(string address, ushort length)
        {
            throw new NotImplementedException();
        }

        public OperateResult<T> Read<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, length, parseResult.FunctionCode);
                
                byte[] result = new byte[length * 2];
                for (int i = 0; i < length; i++)
                {
                    result[i * 2] = (byte)(registers[i] >> 8);
                    result[i * 2 + 1] = (byte)(registers[i] & 0xFF);
                }
                
                return OperateResult<byte[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<byte[]>.CreateFailedResult(ex.Message);
            }
        }

        public Task<OperateResult<T>> ReadAsync<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public OperateResult<bool> ReadBool(string address)
        {
            return ReadBoolAsync(address).GetAwaiter().GetResult();
        }

        public OperateResult<bool> ReadBool(string address, ushort length)
        {
            return ReadBoolAsync(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<bool>> ReadBoolAsync(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                
                if (parseResult.IsBitAccess)
                {
                    // 读取寄存器中的特定位
                    ushort registerValue = await ReadSingleRegisterAsync(parseResult.RegisterAddress, parseResult.FunctionCode);
                    bool result = (registerValue & (1 << parseResult.BitIndex)) != 0;
                    return OperateResult<bool>.CreateSuccessResult(result);
                }
                else
                {
                    // 读取线圈
                    bool result = await ReadSingleCoilAsync(parseResult.RegisterAddress);
                    return OperateResult<bool>.CreateSuccessResult(result);
                }
            }
            catch (Exception ex)
            {
                return OperateResult<bool>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<bool>> ReadBoolAsync(string address, ushort length)
        {
            // 注意：根据接口定义，批量读取布尔值方法返回单个bool值
            // 但这与接口注释不符，这里我们只返回第一个读取的布尔值
            try
            {
                var parseResult = ParseAddress(address);
                
                if (parseResult.IsBitAccess)
                {
                    // 读取寄存器中的特定位
                    int bitIndex = parseResult.BitIndex;
                    ushort currentRegister = (ushort)(parseResult.RegisterAddress + (bitIndex / 16));
                    byte currentBit = (byte)(bitIndex % 16);
                    
                    // 读取当前寄存器值
                    ushort registerValue = await ReadSingleRegisterAsync(currentRegister, parseResult.FunctionCode);
                    bool result = (registerValue & (1 << currentBit)) != 0;
                    
                    return OperateResult<bool>.CreateSuccessResult(result);
                }
                else
                {
                    // 读取单个线圈使用功能码1
                    byte[] requestFrame = BuildModbusTcpFrame(1, parseResult.RegisterAddress, 1);
                    byte[] response = await SendRequestAsync(requestFrame);
                    
                    // 响应格式: 功能码(1) + 字节数(1) + 数据(n)
                    if (response.Length < 3)
                    {
                        throw new InvalidOperationException("Invalid response from Modbus TCP server");
                    }
                    
                    bool result = (response[2] & 0x01) != 0;
                    
                    return OperateResult<bool>.CreateSuccessResult(result);
                }
            }
            catch (Exception ex)
            {
                return OperateResult<bool>.CreateFailedResult(ex.Message);
            }
        }

        #region UInt32
        public OperateResult<uint> ReadUInt32(string address)
        {
            return ReadUInt32Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<uint[]> ReadUInt32(string address, ushort length)
        {
            return ReadUInt32Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<uint>> ReadUInt32Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 2, parseResult.FunctionCode);
                
                byte[] data = new byte[4];
                data[0] = (byte)(registers[0] >> 8);
                data[1] = (byte)(registers[0] & 0xFF);
                data[2] = (byte)(registers[1] >> 8);
                data[3] = (byte)(registers[1] & 0xFF);
                
                uint result = ByteTransform.TransUInt32(data, 0);
                return OperateResult<uint>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<uint>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 2), parseResult.FunctionCode);
                
                uint[] result = new uint[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[4];
                    data[0] = (byte)(registers[i * 2] >> 8);
                    data[1] = (byte)(registers[i * 2] & 0xFF);
                    data[2] = (byte)(registers[i * 2 + 1] >> 8);
                    data[3] = (byte)(registers[i * 2 + 1] & 0xFF);
                    
                    result[i] = ByteTransform.TransUInt32(data, 0);
                }
                
                return OperateResult<uint[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<uint[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region UInt16
        public OperateResult<ushort> ReadUInt16(string address)
        {
            return ReadUInt16Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<ushort[]> ReadUInt16(string address, ushort length)
        {
            return ReadUInt16Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<ushort>> ReadUInt16Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort result = await ReadSingleRegisterAsync(parseResult.RegisterAddress, parseResult.FunctionCode);
                return OperateResult<ushort>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<ushort>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] result = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, length, parseResult.FunctionCode);
                return OperateResult<ushort[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<ushort[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region UInt64
        public OperateResult<ulong> ReadUInt64(string address)
        {
            return ReadUInt64Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<ulong[]> ReadUInt64(string address, ushort length)
        {
            return ReadUInt64Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<ulong>> ReadUInt64Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 4, parseResult.FunctionCode);
                
                byte[] data = new byte[8];
                for (int i = 0; i < 4; i++)
                {
                    data[i * 2] = (byte)(registers[i] >> 8);
                    data[i * 2 + 1] = (byte)(registers[i] & 0xFF);
                }
                
                ulong result = ByteTransform.TransUInt64(data, 0);
                return OperateResult<ulong>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<ulong>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 4), parseResult.FunctionCode);
                
                ulong[] result = new ulong[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[8];
                    for (int j = 0; j < 4; j++)
                    {
                        data[j * 2] = (byte)(registers[i * 4 + j] >> 8);
                        data[j * 2 + 1] = (byte)(registers[i * 4 + j] & 0xFF);
                    }
                    
                    result[i] = ByteTransform.TransUInt64(data, 0);
                }
                
                return OperateResult<ulong[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<ulong[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int32
        public OperateResult<int> ReadInt32(string address)
        {
            return ReadInt32Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<int[]> ReadInt32(string address, ushort length)
        {
            return ReadInt32Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<int>> ReadInt32Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 2, parseResult.FunctionCode);
                
                byte[] data = new byte[4];
                data[0] = (byte)(registers[0] >> 8);
                data[1] = (byte)(registers[0] & 0xFF);
                data[2] = (byte)(registers[1] >> 8);
                data[3] = (byte)(registers[1] & 0xFF);
                
                int result = ByteTransform.TransInt32(data, 0);
                return OperateResult<int>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<int>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 2), parseResult.FunctionCode);
                
                int[] result = new int[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[4];
                    data[0] = (byte)(registers[i * 2] >> 8);
                    data[1] = (byte)(registers[i * 2] & 0xFF);
                    data[2] = (byte)(registers[i * 2 + 1] >> 8);
                    data[3] = (byte)(registers[i * 2 + 1] & 0xFF);
                    
                    result[i] = ByteTransform.TransInt32(data, 0);
                }
                
                return OperateResult<int[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<int[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int16
        public OperateResult<short> ReadInt16(string address)
        {
            return ReadInt16Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<short[]> ReadInt16(string address, ushort length)
        {
            return ReadInt16Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<short>> ReadInt16Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort registerValue = await ReadSingleRegisterAsync(parseResult.RegisterAddress, parseResult.FunctionCode);
                short result = (short)registerValue;
                return OperateResult<short>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<short>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, length, parseResult.FunctionCode);
                
                short[] result = new short[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = (short)registers[i];
                }
                
                return OperateResult<short[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<short[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int64
        public OperateResult<long> ReadInt64(string address)
        {
            return ReadInt64Async(address).GetAwaiter().GetResult();
        }

        public OperateResult<long[]> ReadInt64(string address, ushort length)
        {
            return ReadInt64Async(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<long>> ReadInt64Async(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 4, parseResult.FunctionCode);
                
                byte[] data = new byte[8];
                for (int i = 0; i < 4; i++)
                {
                    data[i * 2] = (byte)(registers[i] >> 8);
                    data[i * 2 + 1] = (byte)(registers[i] & 0xFF);
                }
                
                long result = ByteTransform.TransInt64(data, 0);
                return OperateResult<long>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<long>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 4), parseResult.FunctionCode);
                
                long[] result = new long[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[8];
                    for (int j = 0; j < 4; j++)
                    {
                        data[j * 2] = (byte)(registers[i * 4 + j] >> 8);
                        data[j * 2 + 1] = (byte)(registers[i * 4 + j] & 0xFF);
                    }
                    
                    result[i] = ByteTransform.TransInt64(data, 0);
                }
                
                return OperateResult<long[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<long[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Double
        public OperateResult<double> ReadDouble(string address)
        {
            return ReadDoubleAsync(address).GetAwaiter().GetResult();
        }

        public OperateResult<double[]> ReadDouble(string address, ushort length)
        {
            return ReadDoubleAsync(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<double>> ReadDoubleAsync(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 4, parseResult.FunctionCode);
                
                byte[] data = new byte[8];
                for (int i = 0; i < 4; i++)
                {
                    data[i * 2] = (byte)(registers[i] >> 8);
                    data[i * 2 + 1] = (byte)(registers[i] & 0xFF);
                }
                
                double result = ByteTransform.TransDouble(data, 0);
                return OperateResult<double>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<double>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 4), parseResult.FunctionCode);
                
                double[] result = new double[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[8];
                    for (int j = 0; j < 4; j++)
                    {
                        data[j * 2] = (byte)(registers[i * 4 + j] >> 8);
                        data[j * 2 + 1] = (byte)(registers[i * 4 + j] & 0xFF);
                    }
                    
                    result[i] = ByteTransform.TransDouble(data, 0);
                }
                
                return OperateResult<double[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<double[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Float
        public OperateResult<float> ReadFloat(string address)
        {
            return ReadFloatAsync(address).GetAwaiter().GetResult();
        }

        public OperateResult<float[]> ReadFloat(string address, ushort length)
        {
            return ReadFloatAsync(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<float>> ReadFloatAsync(string address)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, 2, parseResult.FunctionCode);
                
                byte[] data = new byte[4];
                data[0] = (byte)(registers[0] >> 8);
                data[1] = (byte)(registers[0] & 0xFF);
                data[2] = (byte)(registers[1] >> 8);
                data[3] = (byte)(registers[1] & 0xFF);
                
                float result = ByteTransform.TransSingle(data, 0);
            return OperateResult<float>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<float>.CreateFailedResult(ex.Message);
            }
        }

        public async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, (ushort)(length * 2), parseResult.FunctionCode);
                
                float[] result = new float[length];
                for (int i = 0; i < length; i++)
                {
                    byte[] data = new byte[4];
                    data[0] = (byte)(registers[i * 2] >> 8);
                    data[1] = (byte)(registers[i * 2] & 0xFF);
                    data[2] = (byte)(registers[i * 2 + 1] >> 8);
                    data[3] = (byte)(registers[i * 2 + 1] & 0xFF);
                    
                    result[i] = ByteTransform.TransSingle(data, 0);
                }
                
                return OperateResult<float[]>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<float[]>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region String
        public OperateResult<string> ReadString(string address, ushort length)
        {
            return ReadStringAsync(address, length).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, length, parseResult.FunctionCode);
                byte[] bytes = ByteTransform.TransByte(registers);
                string result = ByteTransform.TransString(bytes, 0, length * 2, Encoding.ASCII);
                return OperateResult<string>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<string>.CreateFailedResult(ex.Message);
            }
        }

        public OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
        {
            return ReadStringAsync(address, length, encoding).GetAwaiter().GetResult();
        }

        public async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = await ReadMultipleRegistersAsync(parseResult.RegisterAddress, length, parseResult.FunctionCode);
                byte[] bytes = ByteTransform.TransByte(registers);
                string result = ByteTransform.TransString(bytes, 0, length * 2, encoding);
                return OperateResult<string>.CreateSuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperateResult<string>.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Write Methods
        public ReturnResult Write(string address, bool value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, bool[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, bool value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                
                if (parseResult.IsBitAccess)
                {
                    // 写入寄存器中的特定位
                    ushort registerValue = await ReadSingleRegisterAsync(parseResult.RegisterAddress, parseResult.FunctionCode);
                    
                    if (value)
                    {
                        registerValue |= (ushort)(1 << parseResult.BitIndex);
                    }
                    else
                    {
                        registerValue &= (ushort)~(1 << parseResult.BitIndex);
                    }
                    
                    await WriteSingleRegisterAsync(parseResult.RegisterAddress, registerValue);
                }
                else
                {
                    // 写入线圈
                    await WriteSingleCoilAsync(parseResult.RegisterAddress, value);
                }
                
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, bool[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                
                if (parseResult.IsBitAccess)
                {
                    // 批量写入寄存器中的特定位
                    for (int i = 0; i < value.Length; i++)
                    {
                        int bitIndex = parseResult.BitIndex + i;
                        ushort currentRegister = (ushort)(parseResult.RegisterAddress + (bitIndex / 16));
                        byte currentBit = (byte)(bitIndex % 16);
                        
                        // 读取当前寄存器值
                        ushort registerValue = await ReadSingleRegisterAsync(currentRegister, parseResult.FunctionCode);
                        
                        if (value[i])
                        {
                            registerValue |= (ushort)(1 << currentBit);
                        }
                        else
                        {
                            registerValue &= (ushort)~(1 << currentBit);
                        }
                        
                        // 写入修改后的值
                        await WriteSingleRegisterAsync(currentRegister, registerValue);
                    }
                }
                else
                {
                    // 批量写入线圈使用功能码15
                    byte[] requestFrame = BuildModbusTcpFrame(15, parseResult.RegisterAddress, (ushort)value.Length);
                    
                    // 计算需要的字节数并添加数据
                    int byteCount = (value.Length + 7) / 8;
                    byte[] data = new byte[byteCount];
                    
                    for (int i = 0; i < value.Length; i++)
                    {
                        int byteIndex = i / 8;
                        int bitIndex = i % 8;
                        
                        if (value[i])
                        {
                            data[byteIndex] |= (byte)(1 << bitIndex);
                        }
                    }
                    
                    // 构建完整请求帧：功能码(1) + 起始地址(2) + 数量(2) + 字节数(1) + 数据(n)
                    byte[] fullFrame = new byte[requestFrame.Length + 1 + data.Length];
                    Array.Copy(requestFrame, fullFrame, requestFrame.Length);
                    fullFrame[requestFrame.Length] = (byte)byteCount;
                    Array.Copy(data, 0, fullFrame, requestFrame.Length + 1, data.Length);
                    
                    await SendRequestAsync(fullFrame);
                }
                
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        #region UInt32
        public ReturnResult Write(string address, uint value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, uint[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, uint value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[2];
                registers[0] = (ushort)(data[0] << 8 | data[1]);
                registers[1] = (ushort)(data[2] << 8 | data[3]);
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, uint[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 2;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    registers[i * 2] = (ushort)(data[0] << 8 | data[1]);
                    registers[i * 2 + 1] = (ushort)(data[2] << 8 | data[3]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region UInt16
        public ReturnResult Write(string address, ushort value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, ushort[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, ushort value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                await WriteSingleRegisterAsync(parseResult.RegisterAddress, value);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, ushort[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, value);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region UInt64
        public ReturnResult Write(string address, ulong value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, ulong[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, ulong value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[4];
                for (int i = 0; i < 4; i++)
                {
                    registers[i] = (ushort)(data[i * 2] << 8 | data[i * 2 + 1]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, ulong[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 4;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        registers[i * 4 + j] = (ushort)(data[j * 2] << 8 | data[j * 2 + 1]);
                    }
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int32
        public ReturnResult Write(string address, int value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, int[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, int value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[2];
                registers[0] = (ushort)(data[0] << 8 | data[1]);
                registers[1] = (ushort)(data[2] << 8 | data[3]);
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, int[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 2;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    registers[i * 2] = (ushort)(data[0] << 8 | data[1]);
                    registers[i * 2 + 1] = (ushort)(data[2] << 8 | data[3]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int16
        public ReturnResult Write(string address, short value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, short[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, short value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort registerValue = (ushort)value;
                await WriteSingleRegisterAsync(parseResult.RegisterAddress, registerValue);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, short[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                ushort[] registers = new ushort[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    registers[i] = (ushort)value[i];
                }
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Int64
        public ReturnResult Write(string address, long value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, long[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, long value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[4];
                for (int i = 0; i < 4; i++)
                {
                    registers[i] = (ushort)(data[i * 2] << 8 | data[i * 2 + 1]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, long[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 4;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        registers[i * 4 + j] = (ushort)(data[j * 2] << 8 | data[j * 2 + 1]);
                    }
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Double
        public ReturnResult Write(string address, double value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, double[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, double value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[4];
                for (int i = 0; i < 4; i++)
                {
                    registers[i] = (ushort)(data[i * 2] << 8 | data[i * 2 + 1]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, double[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 4;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        registers[i * 4 + j] = (ushort)(data[j * 2] << 8 | data[j * 2 + 1]);
                    }
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Float
        public ReturnResult Write(string address, float value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public ReturnResult Write(string address, float[] value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, float value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value);
                
                ushort[] registers = new ushort[2];
                registers[0] = (ushort)(data[0] << 8 | data[1]);
                registers[1] = (ushort)(data[2] << 8 | data[3]);
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }

        public async Task<ReturnResult> WriteAsync(string address, float[] value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                int totalRegisters = value.Length * 2;
                ushort[] registers = new ushort[totalRegisters];
                
                for (int i = 0; i < value.Length; i++)
                {
                    byte[] data = ByteTransform.TransByte(value[i]);
                    registers[i * 2] = (ushort)(data[0] << 8 | data[1]);
                    registers[i * 2 + 1] = (ushort)(data[2] << 8 | data[3]);
                }
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region String
        public ReturnResult Write(string address, string value)
        {
            return WriteAsync(address, value).GetAwaiter().GetResult();
        }

        public async Task<ReturnResult> WriteAsync(string address, string value)
        {
            try
            {
                var parseResult = ParseAddress(address);
                byte[] data = ByteTransform.TransByte(value, Encoding.ASCII);
                int totalRegisters = (data.Length + 1) / 2; // 向上取整
                ushort[] registers = ByteTransform.TransUInt16(data, 0, totalRegisters);
                
                await WriteMultipleRegistersAsync(parseResult.RegisterAddress, registers);
                return ReturnResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ReturnResult.CreateFailedResult(ex.Message);
            }
        }
        #endregion

        #region Byte
        public ReturnResult Write(string address, byte value)
        {
            throw new NotImplementedException();
        }

        public ReturnResult Write(string address, byte[] value)
        {
            throw new NotImplementedException();
        }

        public Task<ReturnResult> WriteAsync(string address, byte value)
        {
            throw new NotImplementedException();
        }

        public Task<ReturnResult> WriteAsync(string address, byte[] value)
        {
            throw new NotImplementedException();
        }
        #endregion

        #endregion

        #endregion

        #region IDisposable 实现
        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }

        #endregion
    }
}