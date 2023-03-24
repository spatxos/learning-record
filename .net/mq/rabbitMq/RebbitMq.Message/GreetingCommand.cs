using System;

namespace RebbitMq.Message
{
    public class GreetingCommand
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
    }
}
