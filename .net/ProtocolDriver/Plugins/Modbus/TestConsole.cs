using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Host.SDK;

namespace Modbus
{
    class TestConsole
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing ModbusDriver...");
            
            // 创建Modbus协议连接，使用外部的Modbus服务
            var settings = new Dictionary<string, string>
            {
                { "Host", "localhost" },
                { "Port", "502" },
                { "UnitId", "1" }
            };
            
            var connection = new ModbusProtocolConnection(settings);
            
            try
            {
                // 打开连接
                Console.WriteLine("Opening connection...");
                await connection.OpenAsync();
                Console.WriteLine("Connection opened successfully.");
                
                // 创建请求
                var request = new ProtocolRequest(
                    "read",
                    new Dictionary<string, string>
                    {
                        { "Address", "0" },
                        { "Count", "1" },
                        { "DataType", "int16" }
                    },
                    null
                );
                
                // 执行请求
                Console.WriteLine("Executing request...");
                Console.WriteLine($"Request Action: {request.Action}");
                Console.WriteLine($"Request Address: {request.Props["Address"]}");
                Console.WriteLine($"Request Count: {request.Props["Count"]}");
                Console.WriteLine($"Request DataType: {request.Props["DataType"]}");
                var response = await connection.ExecuteAsync(request);
                
                Console.WriteLine($"Response: Success={response.Success}");
                if (!response.Success)
                {
                    Console.WriteLine($"Error: {response.Error}");
                }
                else
                {
                    Console.WriteLine("Success!");
                    if (response.Parsed != null)
                    {
                        foreach (var kv in response.Parsed)
                        {
                            Console.WriteLine($"  {kv.Key}: {kv.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // 关闭连接
                await connection.CloseAsync();
            }
            
            Console.WriteLine("\nTest completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}