using TrocoPoints.Application.Interfaces;
using TrocoPoints.Application.Services;
using TrocoPoints.Infrastructure.Mongo;
using TrocoPoints.Infrastructure.Persistence;
using TrocoPoints.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
