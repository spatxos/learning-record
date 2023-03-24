using MassTransit;
using RabbitMQ.Extensions;
using System;
using Topshelf.Logging;

namespace RebbitMq.Service
{
    public class GreetingServer
    {
        private readonly IBusControl _bus;
        private readonly LogWriter _log = HostLogger.Get<GreetingServer>();

        public GreetingServer()
        {
            //_log.Info("register consumer");
            _bus = BusCreator.CreateBus((cfg, host) =>
            {

            });
        }

        public void Start()
        {
            //_log.Info("start greeting service");
            _bus.Start();
        }

        public void Stop()
        {
            //_log.Info("stop greeting service");
            _bus.Stop();
        }

    }
}