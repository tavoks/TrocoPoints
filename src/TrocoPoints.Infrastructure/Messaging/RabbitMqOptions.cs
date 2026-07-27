namespace TrocoPoints.Infrastructure.Messaging
{
    public class RabbitMqOptions
    {
        public required string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public required string UserName { get; set; }
        public required string Password { get; set; }

        public required string Exchange { get; set; }
        public required string RetryExchange { get; set; }
        public required string DeadLetterExchange { get; set; }

        public required string Queue { get; set; }
        public required string RetryQueue { get; set; }
        public required string DeadLetterQueue { get; set; }

        public required string RoutingKey { get; set; }

        public int RetryTtlMilliseconds { get; set; } = 30000;
        public int MaxTentativas { get; set; } = 5;
    }
}
