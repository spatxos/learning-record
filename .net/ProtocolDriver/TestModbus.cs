using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modbus;
using Host.SDK;

namespace TestModbus
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing ModbusDriver...");
            
            // 创建ModbusDriver实例
            var driver = new ModbusDriver();
            
            // 设置连接参数
            var settings = new Dictionary<string, string>
            {
                { "Host", "127.0.0.1" },
                { "Port", "502" },
                { "UnitId", "1" }
            };
            
            try
            {
                // 创建连接
                Console.WriteLine("Creating connection...");
                var connection = await driver.CreateConnectionAsync(settings);
                Console.WriteLine("Connection created successfully.");
                
                // 执行读取寄存器操作
                Console.WriteLine("Executing readholdingregisters command...");
                var request = new ProtocolRequest
                {
                    Action = "readholdingregisters",
                    Props = new Dictionary<string, string>
                    {
                        { "Address", "0" },
                        { "Count", "1" }
                    }
                };
                
                var response = await connection.ExecuteAsync(request);
                Console.WriteLine($"Response: Success={response.Success}, Error={response.Error}");
                
                if (response.Success && response.Parsed != null)
                {
                    Console.WriteLine("Parsed data:");
                    foreach (var kv in response.Parsed)
                    {
                        Console.WriteLine($"  {kv.Key}: {kv.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("Test completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}