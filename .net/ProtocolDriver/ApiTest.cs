using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).Wait();
    }

    static async Task MainAsync(string[] args)
    {
        using (var httpClient = new HttpClient())
        {
            string apiUrl = "http://localhost:5001/api/Connection";
            
            // 创建连接
            Console.WriteLine("Creating connection...");
            string connectionJson = "{\"PluginName\":\"Modbus\",\"ProtocolName\":\"ModbusTCP\",\"Host\":\"localhost\",\"Port\":502,\"Parameters\":{\"UnitId\":\"1\"}}";
            
            StringContent connectionContent = new StringContent(connectionJson, Encoding.UTF8, "application/json");
            HttpResponseMessage connectionResponse = await httpClient.PostAsync(apiUrl, connectionContent);
            connectionResponse.EnsureSuccessStatusCode();
            
            string connectionId = await connectionResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Connection created successfully. Connection ID: " + connectionId);
            
            // 执行请求
            Console.WriteLine("\nExecuting request...");
            string requestJson = "{\"ConnectionId\":" + connectionId + ",\"Action\":\"read\",\"Address\":0,\"Count\":1,\"DataType\":\"int16\"}";
            
            StringContent requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            HttpResponseMessage requestResponse = await httpClient.PostAsync(apiUrl + "/Execute", requestContent);
            requestResponse.EnsureSuccessStatusCode();
            
            string responseContent = await requestResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Request executed successfully. Response: " + responseContent);
            
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}