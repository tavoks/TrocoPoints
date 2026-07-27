using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Infrastructure.Messaging;
using TrocoPoints.Infrastructure.Mongo;
using TrocoPoints.Infrastructure.Persistence;
using TrocoPoints.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IContaPontosRepository, ContaPontosRepository>();
builder.Services.AddScoped<IPontosLedgerRepository, PontosLedgerRepository>();

// MongoDB (auditoria) - MongoClient já gerencia um pool de conexões internamente,
// então o repositório pode ser Singleton (diferente do Oracle/Dapper).
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IAuditoriaRepository, MongoAuditoriaRepository>();

// Mensageria (RabbitMQ)
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<RabbitMqTopologyInitializer>();
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("TrocoPoints.Messaging")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:JaegerOtlpEndpoint"]!);
        }));

// HealthChecks - o Worker fala com os 3: Oracle, RabbitMQ e MongoDB.
var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnectionString")!;
var mongoOptions = builder.Configuration.GetSection("MongoDb").Get<MongoDbOptions>()!;
var rabbitOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;
var rabbitConnectionFactory = new RabbitMQ.Client.ConnectionFactory
{
    HostName = rabbitOptions.HostName,
    Port = rabbitOptions.Port,
    UserName = rabbitOptions.UserName,
    Password = rabbitOptions.Password
};

builder.Services.AddHealthChecks()
    .AddOracle(oracleConnectionString, name: "oracle", tags: new[] { "ready" })
    .AddMongoDb(sp => new MongoClient(mongoOptions.ConnectionString), name: "mongodb", tags: new[] { "ready" })
    .AddRabbitMQ(sp => rabbitConnectionFactory.CreateConnectionAsync(), name: "rabbitmq", tags: new[] { "ready" });

var app = builder.Build();

app.UseSerilogRequestLogging();

// Declara a topologia do RabbitMQ (exchanges/filas/bindings) uma vez, na subida do Worker.
// É uma operação idempotente: se já existir com os mesmos parâmetros, não faz nada.
using (var scope = app.Services.CreateScope())
{
    var topologia = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
    await topologia.DeclararTopologiaAsync();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
