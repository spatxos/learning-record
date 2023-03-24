using MassTransit;
using MassTransit.RabbitMqTransport;
using RebbitMq.Service;
using System;

namespace RabbitMQ.Extensions
{
    public static class BusCreator
    {
        public static IBusControl CreateBus(Action<IRabbitMqBusFactoryConfigurator, IRabbitMqHost> registrationAction = null)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(RabbitMqConstants.RabbitMqUri), hst =>
                {
                    hst.Username(RabbitMqConstants.UserName);
                    hst.Password(RabbitMqConstants.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConstants.GreetingQueue, e =>
                {
                    e.Consumer<GreetingConsumer>();
                });
            });
        }
    }

}
