using TrocoPoints.Application.Interfaces;
using TrocoPoints.Infrastructure.Messaging;
using TrocoPoints.Infrastructure.Persistence;
using TrocoPoints.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IContaPontosRepository, ContaPontosRepository>();
builder.Services.AddScoped<IPontosLedgerRepository, PontosLedgerRepository>();

// Mensageria (RabbitMQ)
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<RabbitMqTopologyInitializer>();
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<RabbitMqConsumer>();

var host = builder.Build();

// Declara a topologia do RabbitMQ (exchanges/filas/bindings) uma vez, na subida do Worker.
// É uma operação idempotente: se já existir com os mesmos parâmetros, não faz nada.
using (var scope = host.Services.CreateScope())
{
    var topologia = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
    await topologia.DeclararTopologiaAsync();
}

host.Run();
