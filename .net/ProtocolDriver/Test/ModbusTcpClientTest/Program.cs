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
            Console.WriteLine("=== Modbus TCP驱动测试控制台 ===");
            Console.WriteLine("测试目标: localhost:502");
            Console.WriteLine("测试地址: 100");
            Console.WriteLine("\n操作说明:");
            Console.WriteLine("1. 启动此测试程序");
            Console.WriteLine("2. 手动启动/关闭Modbus服务");
            Console.WriteLine("3. 观察连接状态变化");
            Console.WriteLine("4. 按任意键退出测试\n");

            var modbusDriver = new Modbus.ModbusTcpDriverClient();
            modbusDriver.IPAddress = "127.0.0.1";
            modbusDriver.Port = "502";
            bool isConnected = false;
            int readAddress = 100;

            try
            {
                // 尝试初始连接
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 正在尝试连接到Modbus服务器...");
                try
                {
                    bool tcpConnected = await modbusDriver.ConnectAsync();
                    if (tcpConnected)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 TCP连接建立成功，正在验证Modbus通信...");
                        // 测试一次Modbus通信以确保连接完全正常
                        var testRead = await modbusDriver.ReadBoolAsync(readAddress.ToString());
                        if (testRead.IsSuccess)
                        {
                            isConnected = true;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ 初始连接成功!");
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ TCP连接成功，但Modbus通信失败: {testRead.Message}");
                            isConnected = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ 初始连接失败");
                        isConnected = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ 初始连接失败: {ex.Message}");
                    isConnected = false;
                }

                // 持续监控连接状态并测试读写
                Console.WriteLine("\n=== 开始持续监控 (按任意键停止) ===");
                while (!Console.KeyAvailable)
                {
                    try
                    {
                        // 测试读取功能
                        Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] 尝试读取地址 {readAddress} 的数据...");
                        
                        bool currentReadSuccess = false;
                        try
                        {
                            // 使用IReadWriteNet接口的ReadBoolAsync方法
                            var readResult = await modbusDriver.ReadInt32Async(readAddress.ToString());
                            
                            if (readResult.IsSuccess)
                            {
                                currentReadSuccess = true;
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ 读取成功: {readResult.Content}");
                                
                                // 如果之前是断开状态，现在重新连接成功
                                if (!isConnected)
                                {
                                    isConnected = true;
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🟢 连接状态变更: 已连接");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 读取失败: {readResult.Message}");
                                
                                // 如果之前是连接状态，现在断开了
                                if (isConnected)
                                {
                                    isConnected = false;
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔴 连接状态变更: 已断开  {modbusDriver.IsConnected}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 读取过程中发生异常: {ex.Message}");
                            
                            // 如果之前是连接状态，现在断开了
                            if (isConnected)
                            {
                                isConnected = false;
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔴 连接状态变更: 已断开");
                            }
                        }

                        // 测试写入功能（可选，注释掉可仅测试读取）
                        if (currentReadSuccess) // 只有在读取成功的情况下才尝试写入
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 尝试写入地址 {readAddress} 的数据...");
                            int testValue = DateTime.Now.Second;
                            try
                            {
                                var writeResult = await modbusDriver.WriteAsync(readAddress.ToString(), testValue);
                                
                                if (writeResult.IsSuccess)
                                {
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ 写入成功: 地址 {readAddress} = {testValue}");
                                }
                                else
                                {
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 写入失败: {writeResult.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 写入过程中发生异常: {ex.Message}");
                                
                                // 如果之前是连接状态，现在断开了
                                if (isConnected)
                                {
                                    isConnected = false;
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔴 连接状态变更: 已断开 {modbusDriver.IsConnected}");
                                }
                            }
                        }

                        // 如果连接已断开，尝试重新连接
                        if (!isConnected)
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 正在尝试重新连接...");
                            try
                            {
                                bool tcpReconnected = await modbusDriver.ConnectAsync();
                                if (tcpReconnected)
                                {
                                    // 测试Modbus通信以验证重新连接是否成功
                                    var reconnectTest = await modbusDriver.ReadBoolAsync(readAddress.ToString());
                                    if (reconnectTest.IsSuccess)
                                    {
                                        isConnected = true;
                                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ 重新连接成功!");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ TCP重连成功，但Modbus通信失败: {reconnectTest.Message}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ TCP重连失败");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 重新连接失败: {ex.Message}");
                            }
                        }

                        // 等待一段时间后再次测试
                        await Task.Delay(3000);
                    }
                    catch (Exception ex)
                    {
                        // 如果之前是连接状态，现在断开了
                        if (isConnected)
                        {
                            isConnected = false;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔴 连接状态变更: 已断开 {modbusDriver.IsConnected}");
                        }
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ 通信异常: {ex.Message}");
                        
                        // 等待一段时间后尝试重新连接
                        await Task.Delay(5000);
                        try
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 尝试重新连接...");
                            await modbusDriver.ConnectAsync();
                            isConnected = true;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ 重新连接成功! {modbusDriver.IsConnected}");
                        }
                        catch (Exception connectEx)
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ 重新连接失败: {connectEx.Message}");
                        }
                    }
                }

                // 用户按下了键，退出循环
                Console.WriteLine("\n=== 测试结束 ===");

            }
            finally
            {
                // 关闭连接
                try
                {
                    await modbusDriver.DisconnectAsync();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已关闭连接");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 关闭连接时发生错误: {ex.Message}");
                }
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
