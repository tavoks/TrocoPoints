using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrocoPoints.Application.Dtos.Eventos;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Infrastructure.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<RabbitMqOptions> _options;
        private readonly ILogger<RabbitMqConsumer> _logger;

        public RabbitMqConsumer(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqConsumer> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opcoes = _options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = opcoes.HostName,
                Port = opcoes.Port,
                UserName = opcoes.UserName,
                Password = opcoes.Password
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) => await ProcessarMensagemAsync(channel, opcoes, ea, stoppingToken);

            await channel.BasicConsumeAsync(queue: opcoes.Queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ProcessarMensagemAsync(IChannel channel, RabbitMqOptions opcoes, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var contaPontosRepository = scope.ServiceProvider.GetRequiredService<IContaPontosRepository>();
            var pontosLedgerRepository = scope.ServiceProvider.GetRequiredService<IPontosLedgerRepository>();
            var auditoriaRepository = scope.ServiceProvider.GetRequiredService<IAuditoriaRepository>();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

            var contextoPai = Propagators.DefaultTextMapPropagator.Extract(
                default,
                ea.BasicProperties.Headers,
                (headers, key) => headers!.TryGetValue(key, out var valor) && valor is byte[] bytes
                    ? new[] { Encoding.UTF8.GetString(bytes) }
                    : Enumerable.Empty<string>());

            using var activity = TrocoPointsActivitySource.Instance.StartActivity(
                "processar-mensagem-outbox",
                ActivityKind.Consumer,
                contextoPai.ActivityContext);

            try
            {
                var evento = JsonSerializer.Deserialize<TransacaoRecebidaEvento>(ea.Body.Span)
                    ?? throw new InvalidOperationException("Payload da mensagem inválido.");

                await unitOfWork.IniciarTransacaoAsync();

                var existePontosLedger = await pontosLedgerRepository.ExisteAsync(evento.TransacaoExternaId, stoppingToken);
                if (existePontosLedger)
                {
                    await unitOfWork.RollbackAsync();
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                var contaPontosExistente = await contaPontosRepository.BuscarPorClienteIdAsync(evento.ClienteId, stoppingToken);
                var contaPontosEhNova = contaPontosExistente is null;
                var contaPontos = contaPontosExistente ?? ContaPontos.Criar(evento.ClienteId);

                var pontosCreditados = contaPontos.CreditarPontos(Dinheiro.Criar(evento.Valor));

                if (contaPontosEhNova)
                    await contaPontosRepository.AdicionarAsync(contaPontos, stoppingToken);
                else
                    await contaPontosRepository.AtualizarAsync(contaPontos, stoppingToken);

                var lancamento = PontosLedger.Criar(evento.ClienteId, evento.TransacaoExternaId, pontosCreditados);
                await pontosLedgerRepository.AdicionarAsync(lancamento, stoppingToken);

                await unitOfWork.CommitAsync();

                // Auditoria no MongoDB - fora da transação Oracle (banco separado, dual-write aceito
                // como simplificação de MVP; ver discussão sobre Outbox Pattern também para o Mongo).
                var auditoria = AuditoriaTransacao.Criar(
                    evento.TransacaoExternaId,
                    evento.ClienteId,
                    evento.PdvId,
                    evento.Valor,
                    pontosCreditados,
                    evento.DataHora);
                await auditoriaRepository.AdicionarAsync(auditoria, stoppingToken);

                // Invalida o cache do saldo desse cliente - a próxima consulta busca o valor
                // atualizado no Oracle, em vez de continuar servindo o saldo antigo até o TTL expirar.
                await cache.RemoveAsync($"saldo:cliente:{evento.ClienteId}", stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await TratarFalhaAsync(channel, opcoes, ea, ex, stoppingToken);
            }
        }

        private async Task TratarFalhaAsync(
            IChannel channel,
            RabbitMqOptions opcoes,
            BasicDeliverEventArgs ea,
            Exception ex,
            CancellationToken stoppingToken)
        {
            var tentativas = ContarTentativas(ea.BasicProperties, opcoes.Queue);

            if (tentativas >= opcoes.MaxTentativas)
            {
                _logger.LogError(ex, "Mensagem excedeu {MaxTentativas} tentativas. Movendo para a DLQ final.", opcoes.MaxTentativas);

                await channel.BasicPublishAsync(
                    exchange: opcoes.DeadLetterExchange,
                    routingKey: opcoes.DeadLetterQueue,
                    mandatory: false,
                    basicProperties: new BasicProperties { Persistent = true },
                    body: ea.Body,
                    cancellationToken: stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                return;
            }

            _logger.LogWarning(ex, "Falha ao processar mensagem (tentativa {Tentativas}/{MaxTentativas}). Enviando para retry.", tentativas + 1, opcoes.MaxTentativas);
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }

        private static long ContarTentativas(IReadOnlyBasicProperties properties, string filaPrincipal)
        {
            if (properties.Headers is null || !properties.Headers.TryGetValue("x-death", out var xDeathObj))
                return 0;

            if (xDeathObj is not List<object> mortes)
                return 0;

            foreach (var morteObj in mortes)
            {
                if (morteObj is not Dictionary<string, object> morte)
                    continue;

                var filaDaMorte = morte.TryGetValue("queue", out var queueValor) && queueValor is byte[] queueBytes
                    ? Encoding.UTF8.GetString(queueBytes)
                    : null;

                if (filaDaMorte == filaPrincipal && morte.TryGetValue("count", out var countValor) && countValor is long count)
                    return count;
            }

            return 0;
        }
    }
}
