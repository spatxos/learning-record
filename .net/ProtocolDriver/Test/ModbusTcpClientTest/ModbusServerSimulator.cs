using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ModbusTcpClientTest
{
    public class ModbusServerSimulator : IDisposable
    {
        private readonly TcpListener _listener;
        private bool _running;
        private Task? _serverTask;
        private ushort[] _holdingRegisters = new ushort[1000];
        private ushort[] _inputRegisters = new ushort[1000];

        public int Port { get; }
        public byte UnitId { get; set; } = 1;

        public ModbusServerSimulator(int port = 5026)
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            
            // 初始化一些测试数据
            for (int i = 0; i < _holdingRegisters.Length; i++)
            {
                _holdingRegisters[i] = (ushort)(i + 100);
                _inputRegisters[i] = (ushort)(i + 200);
            }
        }

        public void StartAsync()
        {
            _running = true;
            _listener.Start();
            Console.WriteLine($"📡 Modbus TCP 服务器模拟器已启动，监听端口: {Port}");
            
            _serverTask = Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        _ = HandleClientAsync(client);
                    }
                    catch (Exception) when (!_running)
                    {
                        // 服务器停止时忽略异常
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  服务器错误: {ex.Message}");
                    }
                }
            });
        }

        public async Task StopAsync()
        {
            _running = false;
            _listener.Stop();
            
            if (_serverTask != null)
            {
                await _serverTask;
            }
            
            Console.WriteLine($"📡 Modbus TCP 服务器模拟器已停止");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[256];
                
                while (_running)
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        // 处理Modbus请求
                        byte[] response = ProcessRequest(buffer, bytesRead);
                        if (response != null)
                        {
                            await stream.WriteAsync(response, 0, response.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  客户端处理错误: {ex.Message}");
                        break;
                    }
                }
            }
        }

        private byte[] ProcessRequest(byte[] request, int requestLength)
        {
            // 最小Modbus请求长度: 7字节 (MBAP头) + 1字节 (功能码) = 8字节
            if (requestLength < 8)
            {
                Console.WriteLine("❌ 无效的Modbus请求: 请求长度不足");
                return null;
            }

            // 解析MBAP头
            ushort transactionId = (ushort)(request[0] << 8 | request[1]);
            ushort protocolId = (ushort)(request[2] << 8 | request[3]);
            ushort mbapLength = (ushort)(request[4] << 8 | request[5]);
            byte unitId = request[6];
            byte functionCode = request[7];

            // 检查协议ID和单元ID
            if (protocolId != 0)
            {
                Console.WriteLine("❌ 无效的Modbus请求: 协议ID错误");
                return CreateExceptionResponse(request, functionCode, 0x01); // 非法功能
            }

            if (unitId != UnitId)
            {
                Console.WriteLine("❌ 无效的Modbus请求: 单元ID不匹配");
                return null; // 忽略不匹配的单元ID请求
            }

            Console.WriteLine($"📨 收到请求 - 功能码: 0x{functionCode:X2}");

            switch (functionCode)
            {
                case 0x03: // 读取保持寄存器
                    return HandleReadHoldingRegisters(request, requestLength, transactionId);
                case 0x04: // 读取输入寄存器
                    return HandleReadInputRegisters(request, requestLength, transactionId);
                case 0x10: // 写入多个寄存器
                    return HandleWriteMultipleRegisters(request, requestLength, transactionId);
                default:
                    Console.WriteLine($"❌ 无效的Modbus请求: 不支持的功能码 0x{functionCode:X2}");
                    return CreateExceptionResponse(request, functionCode, 0x01); // 非法功能
            }
        }

        private byte[] HandleReadHoldingRegisters(byte[] request, int requestLength, ushort transactionId)
        {
            if (request.Length < 12)
            {
                return CreateExceptionResponse(request, 0x03, 0x03); // 非法数据值
            }

            ushort startAddress = (ushort)(request[8] << 8 | request[9]);
            ushort registerCount = (ushort)(request[10] << 8 | request[11]);

            Console.WriteLine($"📖 读取保持寄存器 - 起始地址: {startAddress}, 数量: {registerCount}");

            // 检查参数范围
            if (startAddress + registerCount > _holdingRegisters.Length)
            {
                return CreateExceptionResponse(request, 0x03, 0x02); // 非法数据地址
            }

            // 创建响应
            int responseLength = 3 + 2 * registerCount; // 功能码(1) + 字节数(1) + 数据(2*count)
            byte[] response = new byte[7 + responseLength]; // MBAP头(7) + 响应数据

            // 设置MBAP头
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = 0; // 协议ID
            response[3] = 0;
            response[4] = (byte)((responseLength + 1) >> 8); // +1 for unit ID
            response[5] = (byte)((responseLength + 1) & 0xFF);
            response[6] = request[6]; // 单元ID

            // 设置响应数据
            byte functionCode = 0x03; // 读取保持寄存器功能码
            response[7] = functionCode; // 功能码
            response[8] = (byte)(registerCount * 2); // 字节数

            // 填充数据 (big-endian)
            for (int i = 0; i < registerCount; i++)
            {
                ushort value = _holdingRegisters[startAddress + i];
                response[9 + i * 2] = (byte)(value >> 8);
                response[10 + i * 2] = (byte)(value & 0xFF);
            }

            return response;
        }

        private byte[] HandleReadInputRegisters(byte[] request, int requestLength, ushort transactionId)
        {
            if (request.Length < 12)
            {
                return CreateExceptionResponse(request, 0x04, 0x03); // 非法数据值
            }

            ushort startAddress = (ushort)(request[8] << 8 | request[9]);
            ushort registerCount = (ushort)(request[10] << 8 | request[11]);

            Console.WriteLine($"📖 读取输入寄存器 - 起始地址: {startAddress}, 数量: {registerCount}");

            // 检查参数范围
            if (startAddress + registerCount > _inputRegisters.Length)
            {
                return CreateExceptionResponse(request, 0x04, 0x02); // 非法数据地址
            }

            // 创建响应
            int responseLength = 3 + 2 * registerCount; // 功能码(1) + 字节数(1) + 数据(2*count)
            byte[] response = new byte[7 + responseLength]; // MBAP头(7) + 响应数据

            // 设置MBAP头
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = 0; // 协议ID
            response[3] = 0;
            response[4] = (byte)((responseLength + 1) >> 8); // +1 for unit ID
            response[5] = (byte)((responseLength + 1) & 0xFF);
            response[6] = request[6]; // 单元ID

            // 设置响应数据
            byte functionCode = 0x04; // 读取输入寄存器功能码
            response[7] = functionCode; // 功能码
            response[8] = (byte)(registerCount * 2); // 字节数

            // 填充数据 (big-endian)
            for (int i = 0; i < registerCount; i++)
            {
                ushort value = _inputRegisters[startAddress + i];
                response[9 + i * 2] = (byte)(value >> 8);
                response[10 + i * 2] = (byte)(value & 0xFF);
            }

            return response;
        }

        private byte[] HandleWriteMultipleRegisters(byte[] request, int requestLength, ushort transactionId)
        {
            if (request.Length < 13)
            {
                return CreateExceptionResponse(request, 0x10, 0x03); // 非法数据值
            }

            ushort startAddress = (ushort)(request[8] << 8 | request[9]);
            ushort registerCount = (ushort)(request[10] << 8 | request[11]);
            byte byteCount = request[12];

            Console.WriteLine($"📝 写入多个寄存器 - 起始地址: {startAddress}, 数量: {registerCount}");

            // 检查参数范围
            if (startAddress + registerCount > _holdingRegisters.Length)
            {
                return CreateExceptionResponse(request, 0x10, 0x02); // 非法数据地址
            }

            if (byteCount != registerCount * 2)
            {
                return CreateExceptionResponse(request, 0x10, 0x03); // 非法数据值
            }

            if (request.Length < 13 + byteCount)
            {
                return CreateExceptionResponse(request, 0x10, 0x03); // 非法数据值
            }

            // 写入数据
            for (int i = 0; i < registerCount; i++)
            {
                _holdingRegisters[startAddress + i] = (ushort)(request[13 + i * 2] << 8 | request[14 + i * 2]);
                Console.WriteLine($"  地址 {startAddress + i}: {_holdingRegisters[startAddress + i]}");
            }

            // 创建响应
            byte[] response = new byte[15]; // MBAP头(7) + 功能码(1) + 地址(2) + 数量(2)

            // 设置MBAP头
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = 0; // 协议ID
            response[3] = 0;
            response[4] = 0;
            response[5] = 6; // 长度: 单元ID(1) + 功能码(1) + 地址(2) + 数量(2) = 6
            response[6] = request[6]; // 单元ID

            // 设置响应数据
            response[7] = 0x10; // 功能码
            response[8] = request[8]; // 起始地址
            response[9] = request[9];
            response[10] = request[10]; // 寄存器数量
            response[11] = request[11];

            return response;
        }

        private byte[] CreateExceptionResponse(byte[] request, byte functionCode, byte exceptionCode)
        {
            Console.WriteLine($"❌ 返回异常 - 功能码: 0x{functionCode:X2}, 异常码: 0x{exceptionCode:X2}");
            
            byte[] response = new byte[9]; // MBAP头(7) + 功能码(1) + 异常码(1)

            // 设置MBAP头
            response[0] = request[0]; // 事务ID
            response[1] = request[1];
            response[2] = 0; // 协议ID
            response[3] = 0;
            response[4] = 0;
            response[5] = 3; // 长度: 单元ID(1) + 功能码(1) + 异常码(1) = 3
            response[6] = request[6]; // 单元ID

            // 设置异常响应
            response[7] = (byte)(functionCode | 0x80); // 功能码最高位置1表示异常
            response[8] = exceptionCode;

            return response;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _listener.Stop();
        }
    }
}