using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Application.Services;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IContaPontosRepository, ContaPontosRepository>();

// MongoDB (auditoria) - leitura, pelo endpoint de consulta.
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IAuditoriaRepository, MongoAuditoriaRepository>();

// Application (casos de uso)
builder.Services.AddScoped<ReceberTransacaoAppService>();
builder.Services.AddScoped<ConsultarSaldoAppService>();
builder.Services.AddScoped<ConsultarAuditoriaAppService>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("TrocoPoints.Messaging")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:JaegerOtlpEndpoint"]!);
        }));

// HealthChecks - dependências que a Api realmente usa (Oracle e MongoDB; RabbitMQ é só o Worker).
var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnectionString")!;
var mongoOptions = builder.Configuration.GetSection("MongoDb").Get<MongoDbOptions>()!;

builder.Services.AddHealthChecks()
    .AddOracle(oracleConnectionString, name: "oracle", tags: new[] { "ready" })
    .AddMongoDb(sp => new MongoClient(mongoOptions.ConnectionString), name: "mongodb", tags: new[] { "ready" });

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Liveness: só confirma que o processo está de pé, sem checar dependências externas.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: verifica todas as dependências externas (Oracle, MongoDB).
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
