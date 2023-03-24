using MassTransit;
using RebbitMq.Message;
using System;
using System.Threading.Tasks;

namespace RebbitMq.Service
{
    public class GreetingConsumer : IConsumer<GreetingCommand>
    {
        public async Task Consume(ConsumeContext<GreetingCommand> context)
        {
            await Console.Out.WriteLineAsync($"receive greeting commmand: {context.Message.Id},{context.Message.DateTime}");
        }
    }
}
