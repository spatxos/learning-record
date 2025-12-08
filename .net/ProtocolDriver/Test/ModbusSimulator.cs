using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Test
{
    public class ModbusSimulator
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public void Start(int port = 502)
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(System.Net.IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;
            
            Console.WriteLine($"Modbus simulator started on port {port}");
            
            Task.Run(() => AcceptClientsAsync(_cts.Token));
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();
            _listener.Stop();
            Console.WriteLine("Modbus simulator stopped");
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(token);
                    _ = HandleClientAsync(client, token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                
                var stream = client.GetStream();
                var buffer = new byte[1024];
                
                while (_isRunning && !token.IsCancellationRequested && client.Connected)
                {
                    try
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead == 0)
                            break;

                        Console.WriteLine($"Received request: {BitConverter.ToString(buffer, 0, bytesRead)}");
                        
                        // Simple Modbus response for holding register read (function code 03)
                        if (bytesRead >= 8 && buffer[7] == 0x03)
                        {
                            var response = CreateModbusResponse(buffer, bytesRead);
                            await stream.WriteAsync(response, 0, response.Length, token);
                            Console.WriteLine($"Sent response: {BitConverter.ToString(response)}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when stopping
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling client: {ex.Message}");
                        break;
                    }
                }
                
                Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
            }
        }

        private byte[] CreateModbusResponse(byte[] request, int requestLength)
        {
            // Extract request parameters
            var unitId = request[6];
            var functionCode = request[7];
            var startAddress = (ushort)(request[8] << 8 | request[9]);
            var quantity = (ushort)(request[10] << 8 | request[11]);
            
            // Create response
            var responseLength = 9 + quantity * 2;
            var response = new byte[responseLength];
            
            // Transaction ID
            response[0] = request[0];
            response[1] = request[1];
            
            // Protocol ID (always 0 for Modbus TCP)
            response[2] = 0;
            response[3] = 0;
            
            // Length (number of bytes following)
            response[4] = (byte)((quantity * 2 + 2) >> 8);
            response[5] = (byte)((quantity * 2 + 2) & 0xFF);
            
            // Unit ID
            response[6] = unitId;
            
            // Function code
            response[7] = functionCode;
            
            // Byte count
            response[8] = (byte)(quantity * 2);
            
            // Register values (all zeros for simplicity)
            for (int i = 0; i < quantity; i++)
            {
                response[9 + i * 2] = 0;
                response[10 + i * 2] = 0;
            }
            
            return response;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Modbus Simulator");
            Console.WriteLine("================");
            
            var simulator = new ModbusSimulator();
            simulator.Start(502);
            
            Console.WriteLine("Press Enter to stop the simulator...");
            Console.ReadLine();
            
            simulator.Stop();
        }
    }
}