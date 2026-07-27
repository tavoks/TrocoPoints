using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TrocoPoints.Application.Dtos.TransacaoOutbox;
using TrocoPoints.Application.Interfaces;

namespace TrocoPoints.Infrastructure.Messaging
{
    public class OutboxPublisher : BackgroundService
    {
        private static readonly TimeSpan IntervaloEntreExecucoes = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<RabbitMqOptions> _options;
        private readonly ILogger<OutboxPublisher> _logger;

        public OutboxPublisher(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<OutboxPublisher> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _options.Value.HostName,
                Port = _options.Value.Port,
                UserName = _options.Value.UserName,
                Password = _options.Value.Password
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

                try
                {
                    await unitOfWork.IniciarTransacaoAsync();

                    var mensagensPendentes = await outboxRepository.BuscarPendentesAsync(50, stoppingToken);
                    var idsPublicadosComSucesso = new List<Guid>();

                    foreach (var mensagem in mensagensPendentes)
                    {
                        try
                        {
                            var corpo = Encoding.UTF8.GetBytes(mensagem.Payload);
                            var propriedades = new BasicProperties { Persistent = true };

                            await channel.BasicPublishAsync(
                                exchange: _options.Value.Exchange,
                                routingKey: mensagem.TipoEvento.ParaRoutingKey(),
                                mandatory: false,
                                basicProperties: propriedades,
                                body: corpo,
                                cancellationToken: stoppingToken);

                            idsPublicadosComSucesso.Add(mensagem.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Falha ao publicar mensagem outbox {MensagemId}. Será tentado novamente no próximo ciclo.", mensagem.Id);
                        }
                    }

                    if (idsPublicadosComSucesso.Count > 0)
                        await outboxRepository.MarcarComoProcessadasAsync(idsPublicadosComSucesso, stoppingToken);

                    await unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar o lote de mensagens da outbox.");
                    await unitOfWork.RollbackAsync();
                }

                await Task.Delay(IntervaloEntreExecucoes, stoppingToken);
            }
        }
    }
}
