using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService1.Services
{
    public class StreamExChangeService: StreamExChange.StreamExChangeBase
    {
        private readonly ILogger<StreamExChangeService> _logger;
        public StreamExChangeService(ILogger<StreamExChangeService> logger)
        {
            _logger = logger;
        }

        public override async Task GetStream(StreamReqData request, IServerStreamWriter<StreamResData> responseStream, ServerCallContext context)
        {
            var i = 0;
            while (i <= 10)
            {
                i++;

                Console.WriteLine(i);

                Console.WriteLine(request.Data);

                await responseStream.WriteAsync(new StreamResData() { Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

                await Task.Delay(1300);//此处主要是为了方便客户端能看出流调用的效果
            }
        }
        public override async Task<StreamResData> PutStream(IAsyncStreamReader<StreamReqData> requestStream, ServerCallContext context)
        {
            for (int i = 0; i < 10; i++)
            {
                await requestStream.ReadAllAsync(new BathTheCatReq() { Id = rand.Next(0, catCount.Count) });
            }
            //发送完毕
            await requestStream.();

            return new StreamResData() { Data = "收到之后请回复，over！" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        }

        public override async Task AllStream(IAsyncStreamReader<StreamReqData> requestStream, IServerStreamWriter<StreamResData> responseStream, ServerCallContext context)
        {
            await GetStream(requestStream.Current, responseStream, context);

            await PutStream(requestStream, context);
        }
    }
}
