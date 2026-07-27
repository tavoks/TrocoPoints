using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace TrocoPoints.Infrastructure.Messaging
{
    public class RabbitMqTopologyInitializer
    {
        private readonly IOptions<RabbitMqOptions> _options;
        public RabbitMqTopologyInitializer(IOptions<RabbitMqOptions> options)
        {
            _options = options;
        }

        public async Task DeclararTopologiaAsync()
        {
            var opcoes = _options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = opcoes.HostName,
                Port = opcoes.Port,
                UserName = opcoes.UserName,
                Password = opcoes.Password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // 1. Exchanges
            await channel.ExchangeDeclareAsync(exchange: opcoes.Exchange, type: ExchangeType.Topic, durable: true);
            await channel.ExchangeDeclareAsync(exchange: opcoes.RetryExchange, type: ExchangeType.Direct, durable: true);
            await channel.ExchangeDeclareAsync(exchange: opcoes.DeadLetterExchange, type: ExchangeType.Fanout, durable: true);

            // 2. Filas
            await channel.QueueDeclareAsync(
                queue: opcoes.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", opcoes.RetryExchange },
                    { "x-dead-letter-routing-key", opcoes.RetryQueue }
                }
            );

            await channel.QueueDeclareAsync(
                queue: opcoes.RetryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", opcoes.Exchange },
                    { "x-dead-letter-routing-key", opcoes.RoutingKey },
                    { "x-message-ttl", opcoes.RetryTtlMilliseconds }
                }
            );

            await channel.QueueDeclareAsync(
                queue: opcoes.DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // 3. Bindings
            await channel.QueueBindAsync(queue: opcoes.Queue, exchange: opcoes.Exchange, routingKey: opcoes.RoutingKey);
            await channel.QueueBindAsync(queue: opcoes.RetryQueue, exchange: opcoes.RetryExchange, routingKey: opcoes.RetryQueue);
            await channel.QueueBindAsync(queue: opcoes.DeadLetterQueue, exchange: opcoes.DeadLetterExchange, routingKey: opcoes.DeadLetterQueue);
        }
    }
}
