using Host.SDK;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>
    /// Modbus TCP驱动客户端实现
    /// </summary>
    public class ModbusTcpDriverClient : IDriverClient
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private string _ipAddress = string.Empty;
        private int _port = 502;
        private byte _unitId = 1;

        /// <summary>
        /// 连接到Modbus TCP设备
        /// </summary>
        public async Task<bool> ConnectAsync(string ipAddress, int port, byte unitId, CancellationToken token = default)
        {
            try
            {
                // 断开现有连接
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    await DisconnectAsync();
                }

                _ipAddress = ipAddress;
                _port = port;
                _unitId = unitId;

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ipAddress, port, token);
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
                    _networkStream.Close();
                    _networkStream.Dispose();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
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
        /// 获取当前连接状态
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _tcpClient != null && _tcpClient.Connected;
            }
        }

        /// <summary>
        /// 读取Modbus设备数据
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="address">设备地址，格式："功能码@起始地址"，例如"3@100"表示读取保持寄存器起始地址100</param>
        /// <param name="count">读取数量</param>
        /// <param name="token">取消令牌</param>
        /// <returns>读取的数据数组</returns>
        public async Task<T[]> Read<T, TRequest>(TRequest request, CancellationToken token = default) where TRequest : ReadRequestBase
        {
            if (_tcpClient == null || !_tcpClient.Connected)
            {
                throw new InvalidOperationException("Not connected to Modbus TCP server");
            }

            // 根据数据类型调整寄存器数量
            int requiredRegisters = request.Count;
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                requiredRegisters = request.Count * 2;
            }
            else if (typeof(T) == typeof(double))
            {
                requiredRegisters = request.Count * 4;
            }

            // 构建Modbus请求
            var modbusRequest = new ModbusRequest
            {
                FunctionCode = request.FunctionCode,
                StartAddress = request.StartAddress,
                Count = (ushort)requiredRegisters
            };

            // 构建Modbus TCP帧
            byte[] requestFrame = BuildModbusTcpFrame((byte)request.FunctionCode, modbusRequest);

            // 发送请求
            await _networkStream!.WriteAsync(requestFrame, 0, requestFrame.Length, token);
            await _networkStream.FlushAsync(token);

            // 接收响应
            byte[] responseBuffer = new byte[1024];
            int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);

            if (bytesRead < 9)
            {
                throw new InvalidOperationException("Invalid response from Modbus TCP server");
            }

            // 提取Modbus PDU
            int dataLength = (responseBuffer[4] << 8) | responseBuffer[5];
            byte[] pdu = new byte[dataLength - 1];
            // 正确的PDU起始位置应该是索引7（单元ID的下一个字节），而不是索引8
            Array.Copy(responseBuffer, 7, pdu, 0, pdu.Length);

            // 检查是否是错误响应
            if ((pdu[0] & 0x80) != 0)
            {
                throw new Exception($"Modbus error: {pdu[1]}");
            }

            // 解析响应数据
            return ParseResponse<T>(pdu, request.Count);
        }

        /// <summary>
        /// 写入Modbus设备数据
        /// </summary>
        /// <typeparam name="T">写入数据类型</typeparam>
        /// <param name="address">设备地址，格式："功能码@起始地址"，例如"6@100"表示写入保持寄存器起始地址100</param>
        /// <param name="values">写入的数据</param>
        /// <param name="token">取消令牌</param>
        /// <returns>写入是否成功</returns>
        public async Task<bool> Write<TRequest>(TRequest request, CancellationToken token = default) where TRequest : WriteRequestBase
        {
            if (_tcpClient == null || !_tcpClient.Connected)
            {
                throw new InvalidOperationException("Not connected to Modbus TCP server");
            }

            int functionCode = request.FunctionCode;
            ushort startAddress = request.StartAddress;

            // 将功能码6（写入单个寄存器）转换为功能码16（写入多个寄存器）的单个寄存器写入
            if (functionCode == 6)
            {
                functionCode = 16;
            }

            // 使用请求中的数据直接
            byte[] dataBytes = request.Data;

            // 构建Modbus请求
            var modbusRequest = new ModbusRequest
            {
                FunctionCode = functionCode,
                StartAddress = startAddress,
                Count = (ushort)(dataBytes.Length / 2), // 每个寄存器2个字节
                Data = dataBytes
            };

            // 构建Modbus TCP帧
            byte[] requestFrame = BuildModbusTcpFrame((byte)functionCode, modbusRequest);

            // 发送请求
            await _networkStream!.WriteAsync(requestFrame, 0, requestFrame.Length, token);
            await _networkStream.FlushAsync(token);

            // 接收响应
            byte[] responseBuffer = new byte[1024];
            int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);

            if (bytesRead < 9)
            {
                return false;
            }

            // 提取Modbus PDU
            int dataLength = (responseBuffer[4] << 8) | responseBuffer[5];
            byte[] pdu = new byte[dataLength - 1];
            // 正确的PDU起始位置应该是索引7（单元ID的下一个字节），而不是索引8
            Array.Copy(responseBuffer, 7, pdu, 0, pdu.Length);

            // 检查功能码是否匹配
            return pdu[0] == functionCode;
        }

        /// <summary>
        /// 构建Modbus TCP帧
        /// </summary>
        private byte[] BuildModbusTcpFrame(byte functionCode, ModbusRequest modbusRequest)
        {
            byte[] pdu;

            switch ((ModbusFunctionCode)functionCode)
            {
                case ModbusFunctionCode.ReadCoils:
                case ModbusFunctionCode.ReadDiscreteInputs:
                case ModbusFunctionCode.ReadHoldingRegisters:
                case ModbusFunctionCode.ReadInputRegisters:
                    // 读取请求PDU：功能码(1) + 起始地址(2) + 数量(2)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    pdu[3] = (byte)(modbusRequest.Count >> 8);
                    pdu[4] = (byte)(modbusRequest.Count & 0xFF);
                    break;

                case ModbusFunctionCode.WriteSingleCoil:
                    // 写入单个线圈PDU：功能码(1) + 地址(2) + 值(2)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    pdu[3] = (modbusRequest.Data != null && modbusRequest.Data[0] != 0) ? (byte)0xFF : (byte)0x00;
                    pdu[4] = 0x00;
                    break;

                case ModbusFunctionCode.WriteSingleRegister:
                    // 写入单个寄存器PDU：功能码(1) + 地址(2) + 值(2)
                    pdu = new byte[5];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    if (modbusRequest.Data != null && modbusRequest.Data.Length >= 2)
                    {
                        pdu[3] = modbusRequest.Data[0];
                        pdu[4] = modbusRequest.Data[1];
                    }
                    else
                    {
                        pdu[3] = 0x00;
                        pdu[4] = 0x00;
                    }
                    break;

                case ModbusFunctionCode.WriteMultipleCoils:
                    // 写入多个线圈PDU：功能码(1) + 起始地址(2) + 数量(2) + 字节数(1) + 数据
                    if (modbusRequest.Data == null)
                    {
                        throw new InvalidOperationException("No data provided for write operation");
                    }

                    int coilBytes = (int)Math.Ceiling(modbusRequest.Count / 8.0);
                    pdu = new byte[6 + coilBytes];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    pdu[3] = (byte)(modbusRequest.Count >> 8);
                    pdu[4] = (byte)(modbusRequest.Count & 0xFF);
                    pdu[5] = (byte)coilBytes;
                    Array.Copy(modbusRequest.Data, 0, pdu, 6, Math.Min(coilBytes, modbusRequest.Data.Length));
                    break;

                case ModbusFunctionCode.WriteMultipleRegisters:
                    // 写入多个寄存器PDU：功能码(1) + 起始地址(2) + 数量(2) + 字节数(1) + 数据
                    if (modbusRequest.Data == null)
                    {
                        throw new InvalidOperationException("No data provided for write operation");
                    }

                    pdu = new byte[6 + modbusRequest.Data.Length];
                    pdu[0] = functionCode;
                    pdu[1] = (byte)(modbusRequest.StartAddress >> 8);
                    pdu[2] = (byte)(modbusRequest.StartAddress & 0xFF);
                    pdu[3] = (byte)(modbusRequest.Count >> 8);
                    pdu[4] = (byte)(modbusRequest.Count & 0xFF);
                    pdu[5] = (byte)modbusRequest.Data.Length;
                    Array.Copy(modbusRequest.Data, 0, pdu, 6, modbusRequest.Data.Length);
                    break;

                default:
                    throw new NotImplementedException($"Function code {functionCode} not implemented");
            }

            // 构建完整Modbus TCP帧
            byte[] frame = new byte[7 + pdu.Length];
            frame[0] = 0x00; // 事务标识符高字节
            frame[1] = 0x01; // 事务标识符低字节
            frame[2] = 0x00; // 协议标识符高字节
            frame[3] = 0x00; // 协议标识符低字节
            frame[4] = (byte)((pdu.Length + 1) >> 8); // 长度高字节
            frame[5] = (byte)((pdu.Length + 1) & 0xFF); // 长度低字节
            frame[6] = _unitId; // 单元标识符
            Array.Copy(pdu, 0, frame, 7, pdu.Length);

            return frame;
        }

        /// <summary>
        /// 根据类型解析Modbus响应
        /// </summary>
        private T[] ParseResponse<T>(byte[] pdu, int count)
        {
            Type type = typeof(T);
            var result = new T[count];

            // 跳过功能码和字节计数
            int dataOffset = 2;

            if (type == typeof(bool))
            {
                // 解析布尔值（线圈/离散输入）
                bool[] boolResult = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    boolResult[i] = (pdu[dataOffset + byteIndex] & (1 << bitIndex)) != 0;
                }
                return boolResult as T[];
            }
            else if (type == typeof(ushort))
            {
                // 解析无符号16位整数
                ushort[] ushortResult = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 2;
                    ushortResult[i] = (ushort)((pdu[index] << 8) | pdu[index + 1]);
                }
                return ushortResult as T[];
            }
            else if (type == typeof(short))
            {
                // 解析有符号16位整数
                short[] shortResult = new short[count];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 2;
                    shortResult[i] = (short)((pdu[index] << 8) | pdu[index + 1]);
                }
                return shortResult as T[];
            }
            else if (type == typeof(uint))
            {
                // 解析无符号32位整数
                uint[] uintResult = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 4;
                    uintResult[i] = (uint)((pdu[index] << 24) | (pdu[index + 1] << 16) | (pdu[index + 2] << 8) | pdu[index + 3]);
                }
                return uintResult as T[];
            }
            else if (type == typeof(int))
            {
                // 解析有符号32位整数
                int[] intResult = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 4;
                    intResult[i] = (pdu[index] << 24) | (pdu[index + 1] << 16) | (pdu[index + 2] << 8) | pdu[index + 3];
                }
                return intResult as T[];
            }
            else if (type == typeof(float))
            {
                // 解析32位浮点数
                float[] floatResult = new float[count];
                byte[] floatBytes = new byte[4];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 4;
                    // Modbus使用big-endian格式，需要转换为little-endian格式供BitConverter使用
                    floatBytes[3] = pdu[index];
                    floatBytes[2] = pdu[index + 1];
                    floatBytes[1] = pdu[index + 2];
                    floatBytes[0] = pdu[index + 3];
                    floatResult[i] = BitConverter.ToSingle(floatBytes, 0);
                }
                return floatResult as T[];
            }
            else if (type == typeof(double))
            {
                // 解析64位浮点数
                double[] doubleResult = new double[count];
                byte[] doubleBytes = new byte[8];
                for (int i = 0; i < count; i++)
                {
                    int index = dataOffset + i * 8;
                    // Modbus使用big-endian格式，需要转换为little-endian格式供BitConverter使用
                    doubleBytes[7] = pdu[index];
                    doubleBytes[6] = pdu[index + 1];
                    doubleBytes[5] = pdu[index + 2];
                    doubleBytes[4] = pdu[index + 3];
                    doubleBytes[3] = pdu[index + 4];
                    doubleBytes[2] = pdu[index + 5];
                    doubleBytes[1] = pdu[index + 6];
                    doubleBytes[0] = pdu[index + 7];
                    doubleResult[i] = BitConverter.ToDouble(doubleBytes, 0);
                }
                return doubleResult as T[];
            }
            else
            {
                throw new NotImplementedException($"Type {type.Name} not supported for Modbus response parsing");
            }
        }

        /// <summary>
        /// 将数据转换为字节数组
        /// </summary>
        private byte[] ConvertToBytes<T>(T[] values)
        {
            Type type = typeof(T);
            int elementSize = GetElementSize(type);
            byte[] result = new byte[values.Length * elementSize];

            for (int i = 0; i < values.Length; i++)
            {
                int offset = i * elementSize;

                if (type == typeof(bool))
                {
                    // 布尔值转换为位
                    int byteIndex = offset / 8;
                    int bitIndex = offset % 8;
                    if ((bool)(object)values[i])
                    {
                        result[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
                else if (type == typeof(ushort))
                {
                    // 无符号16位整数（big-endian）
                    ushort value = (ushort)(object)values[i];
                    result[offset] = (byte)(value >> 8);
                    result[offset + 1] = (byte)(value & 0xFF);
                }
                else if (type == typeof(short))
                {
                    // 有符号16位整数（big-endian）
                    short value = (short)(object)values[i];
                    result[offset] = (byte)(value >> 8);
                    result[offset + 1] = (byte)(value & 0xFF);
                }
                else if (type == typeof(uint))
                {
                    // 无符号32位整数（big-endian）
                    uint value = (uint)(object)values[i];
                    result[offset] = (byte)(value >> 24);
                    result[offset + 1] = (byte)(value >> 16);
                    result[offset + 2] = (byte)(value >> 8);
                    result[offset + 3] = (byte)(value & 0xFF);
                }
                else if (type == typeof(int))
                {
                    // 有符号32位整数（big-endian）
                    int value = (int)(object)values[i];
                    result[offset] = (byte)(value >> 24);
                    result[offset + 1] = (byte)(value >> 16);
                    result[offset + 2] = (byte)(value >> 8);
                    result[offset + 3] = (byte)(value & 0xFF);
                }
                else if (type == typeof(float))
                {
                    // 32位浮点数（big-endian）
                    float value = (float)(object)values[i];
                    byte[] bytes = BitConverter.GetBytes(value);
                    result[offset] = bytes[3];
                    result[offset + 1] = bytes[2];
                    result[offset + 2] = bytes[1];
                    result[offset + 3] = bytes[0];
                }
                else if (type == typeof(double))
                {
                    // 64位浮点数（big-endian）
                    double value = (double)(object)values[i];
                    byte[] bytes = BitConverter.GetBytes(value);
                    result[offset] = bytes[7];
                    result[offset + 1] = bytes[6];
                    result[offset + 2] = bytes[5];
                    result[offset + 3] = bytes[4];
                    result[offset + 4] = bytes[3];
                    result[offset + 5] = bytes[2];
                    result[offset + 6] = bytes[1];
                    result[offset + 7] = bytes[0];
                }
            }

            return result;
        }

        /// <summary>
        /// 获取类型的字节大小
        /// </summary>
        private int GetElementSize(Type type)
        {
            if (type == typeof(bool)) return 1;
            if (type == typeof(ushort)) return 2;
            if (type == typeof(short)) return 2;
            if (type == typeof(uint)) return 4;
            if (type == typeof(int)) return 4;
            if (type == typeof(float)) return 4;
            if (type == typeof(double)) return 8;
            throw new NotImplementedException($"Type {type.Name} not supported");
        }
    }
}