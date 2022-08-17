using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1;
using System;
using System.Threading.Tasks;

namespace gRPCClient1
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var streamClient = new StreamExChange.StreamExChangeClient(channel);

            var replyGet = streamClient.GetStream(new StreamReqData() { Data = "replyGet" });
            var replyGetRespTask = Task.Run(async () =>
            {
                await foreach (var resp in replyGet.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine("replyGet 服务返回数据: " + resp.Data);
                }
            });
            Console.WriteLine("replyGet 服务返回数据开始: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await replyGetRespTask;
            Console.WriteLine("replyGet 服务返回数据完毕 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var replyPut = streamClient.PutStream();
            await replyPut.RequestStream.WriteAsync(new StreamReqData() { Data = $"我收到了你说的：{replyPut.ResponseAsync.Result.Data}，回你{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}" });


            //var client = new Greeter.GreeterClient(channel);
            //var reply = await client.SayHelloAsync(
            //    new HelloRequest { Name = "晓晨" });
            //Console.WriteLine("Greeter 服务返回数据: " + reply.Message);

            //var catClient = new LuCat.LuCatClient(channel);
            ////获取猫总数
            //var catCount = await catClient.CountAsync(new Empty());
            //Console.WriteLine($"一共{catCount.Count}只猫。");
            //var rand = new Random(DateTime.Now.Millisecond);

            //var bathCat = catClient.BathTheCat();
            ////定义接收吸猫响应逻辑
            //var bathCatRespTask = Task.Run(async () =>
            //{
            //    await foreach (var resp in bathCat.ResponseStream.ReadAllAsync())
            //    {
            //        Console.WriteLine(resp.Message);
            //    }
            //});
            ////随机给10个猫洗澡
            //for (int i = 0; i < 10; i++)
            //{
            //    await bathCat.RequestStream.WriteAsync(new BathTheCatReq() { Id = rand.Next(0, catCount.Count) });
            //}
            ////发送完毕
            //await bathCat.RequestStream.CompleteAsync();
            //Console.WriteLine("客户端已发送完10个需要洗澡的猫id");
            //Console.WriteLine("接收洗澡结果：");
            ////开始接收响应
            //await bathCatRespTask;

            //Console.WriteLine("洗澡完毕");

            Console.ReadKey();
        }
    }
}
