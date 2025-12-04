using System;
using System.Threading.Tasks;
using System.Linq;
using Host.SDK;
using Modbus;

namespace ModbusTcpClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string ipAddress = "127.0.0.1";
            int port = 5026;
            byte unitId = 1;

            Console.WriteLine("=== Modbus TCP Client 测试程序 ===");
            Console.WriteLine($"测试服务器地址: {ipAddress}:{port}");
            Console.WriteLine();

            // 创建并启动Modbus服务器模拟器
            // Console.WriteLine("0. 正在启动Modbus服务器模拟器...");
            // var server = new ModbusServerSimulator(port);
            // server.StartAsync();
            // // 等待服务器启动
            // await Task.Delay(100);
            // Console.WriteLine();

            // 创建Modbus TCP驱动客户端实例
            var client = new ModbusTcpDriverClient();

            try
            {
                // 连接到服务器
                Console.WriteLine("1. 正在连接到Modbus TCP服务器...");
                bool connected = await client.ConnectAsync(ipAddress, port, unitId);
                if (connected)
                {
                    Console.WriteLine("✅ 连接成功！");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("❌ 连接失败！");
                    return;
                }

                // 测试不同类型数据的读取
                Console.WriteLine("2. 测试数据读取功能");
                Console.WriteLine(new string('-', 50));

                // 测试读取保持寄存器 (功能码0x03)
                ushort startAddress = 100;
                int count = 1;

                Console.WriteLine($"\n测试读取保持寄存器: 地址={startAddress}, 数量={count}");
                Console.WriteLine(new string('-', 40));

                // 测试读取不同类型的数据
                Console.WriteLine("\n读取 ushort 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    ushort[] uint16Values = await client.Read<ushort, ModbusReadRequest>(readRequest);
                    if (uint16Values != null && uint16Values.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {uint16Values[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                Console.WriteLine("\n读取 short 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    short[] int16Values = await client.Read<short, ModbusReadRequest>(readRequest);
                    if (int16Values != null && int16Values.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {int16Values[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                Console.WriteLine("\n读取 uint 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    uint[] uint32Values = await client.Read<uint, ModbusReadRequest>(readRequest);
                    if (uint32Values != null && uint32Values.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {uint32Values[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                Console.WriteLine("\n读取 int 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    int[] int32Values = await client.Read<int, ModbusReadRequest>(readRequest);
                    if (int32Values != null && int32Values.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {int32Values[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                Console.WriteLine("\n读取 float 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    float[] floatValues = await client.Read<float, ModbusReadRequest>(readRequest);
                    if (floatValues != null && floatValues.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {floatValues[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                Console.WriteLine("\n读取 double 类型:");
                try
                {
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    double[] doubleValues = await client.Read<double, ModbusReadRequest>(readRequest);
                    if (doubleValues != null && doubleValues.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 结果: {doubleValues[0]}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 读取结果为空");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                // 测试写入功能
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("3. 测试数据写入功能");
                Console.WriteLine(new string('-', 50));

                Console.WriteLine($"\n测试写入保持寄存器: 地址={startAddress}");
                Console.WriteLine(new string('-', 40));

                // 测试写入不同类型的数据
                ushort writeValue = 12345;
                Console.WriteLine($"\n写入 ushort 类型值: {writeValue}");
                try
                {
                    // 将ushort转换为字节数组（大端序）
                    byte[] writeData = new byte[2];
                    writeData[0] = (byte)(writeValue >> 8);
                    writeData[1] = (byte)(writeValue & 0xFF);

                    var writeRequest = new ModbusWriteRequest
                    {
                        FunctionCode = 6,
                        StartAddress = startAddress,
                        Data = writeData
                    };

                    await client.Write(writeRequest);
                    Console.WriteLine("  ✅ 写入成功！");

                    // 验证写入结果
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = count
                    };
                    ushort[] readBack = await client.Read<ushort, ModbusReadRequest>(readRequest);
                    if (readBack != null && readBack.Length > 0)
                    {
                        Console.WriteLine($"  ✅ 验证读取结果: {readBack[0]} ({(readBack[0] == writeValue ? "一致" : "不一致")})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

                // 测试写入多个寄存器
                ushort[] writeValues = { 6789, 9876 };
                Console.WriteLine($"\n写入多个 ushort 类型值: {string.Join(", ", writeValues)}");
                try
                {
                    // 将ushort数组转换为字节数组（大端序）
                    byte[] writeData = new byte[writeValues.Length * 2];
                    for (int i = 0; i < writeValues.Length; i++)
                    {
                        writeData[i * 2] = (byte)(writeValues[i] >> 8);
                        writeData[i * 2 + 1] = (byte)(writeValues[i] & 0xFF);
                    }

                    var writeRequest = new ModbusWriteRequest
                    {
                        FunctionCode = 16,
                        StartAddress = startAddress,
                        Data = writeData
                    };

                    await client.Write(writeRequest);
                    Console.WriteLine("  ✅ 批量写入成功！");

                    // 验证写入结果
                    var readRequest = new ModbusReadRequest
                    {
                        FunctionCode = 3,
                        StartAddress = startAddress,
                        Count = writeValues.Length
                    };
                    ushort[] readBackValues = await client.Read<ushort, ModbusReadRequest>(readRequest);
                    Console.WriteLine("  验证读取结果:");
                    if (readBackValues != null && readBackValues.Length == writeValues.Length)
                    {
                        for (int i = 0; i < writeValues.Length; i++)
                        {
                            Console.WriteLine($"    地址 {startAddress + i}: {readBackValues[i]} ({(readBackValues[i] == writeValues[i] ? "一致" : "不一致")})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  ❌ 验证读取结果失败");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ 失败: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 测试失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // 断开连接
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("4. 断开连接");
                try
                {
                    bool disconnected = await client.DisconnectAsync();
                    Console.WriteLine(disconnected ? "✅ 断开连接成功！" : "❌ 断开连接失败！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 断开连接失败: {ex.Message}");
                }

                // 停止Modbus服务器模拟器
                // Console.WriteLine("\n5. 停止Modbus服务器模拟器...");
                // await server.StopAsync();
            }

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("测试完成，按任意键退出...");
            Console.ReadKey();
        }
    }
}
