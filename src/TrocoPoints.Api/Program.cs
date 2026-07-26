using TrocoPoints.Application.Interfaces;
using TrocoPoints.Application.Services;
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

// Application (casos de uso)
builder.Services.AddScoped<ReceberTransacaoAppService>();
builder.Services.AddScoped<ConsultarSaldoAppService>();

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
