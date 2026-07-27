using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;
using TrocoPoints.Application.Dtos.Requests;
using TrocoPoints.Application.Services;
using TrocoPoints.Domain.ValueObjects;
using TrocoPoints.Infrastructure.Persistence;
using TrocoPoints.Infrastructure.Repositories;

namespace TrocoPoints.IntegrationTests
{
    public class ReceberTransacaoIdempotenciaTests : IAsyncLifetime
    {
        private readonly OracleContainer _oracle = new OracleBuilder().Build();
        private string _connectionString = string.Empty;

        public async Task InitializeAsync()
        {
            await _oracle.StartAsync();
            _connectionString = _oracle.GetConnectionString();

            await CriarEsquemaAsync();
        }

        public Task DisposeAsync() => _oracle.DisposeAsync().AsTask();

        private async Task CriarEsquemaAsync()
        {
            // Mesmas tabelas do docker/init-db, sem o ALTER SESSION SET CONTAINER/CURRENT_SCHEMA -
            // o Testcontainers já conecta direto no schema do usuário de app, sem a complexidade
            // de CDB/PDB que tivemos que resolver manualmente no docker-compose.
            string[] comandos =
            [
                """
                CREATE TABLE Clientes (
                    Id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    Cpf VARCHAR2(11) NOT NULL,
                    Nome VARCHAR2(200) NULL,
                    CONSTRAINT UQ_Clientes_Cpf UNIQUE (Cpf)
                )
                """,
                """
                CREATE TABLE Transacoes (
                    Id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    ClienteId NUMBER NOT NULL,
                    Dinheiro NUMBER(10,2) NOT NULL,
                    PdvId VARCHAR2(50) NOT NULL,
                    TransacaoExternaId VARCHAR2(36) NOT NULL,
                    DataHora TIMESTAMP NOT NULL,
                    CONSTRAINT FK_Transacoes_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
                    CONSTRAINT UQ_Transacoes_TransacaoExternaId UNIQUE (TransacaoExternaId)
                )
                """,
                """
                CREATE TABLE OutboxMessages (
                    Id VARCHAR2(36) PRIMARY KEY,
                    TipoEvento VARCHAR2(100) NOT NULL,
                    Payload CLOB NOT NULL,
                    DataCriacao TIMESTAMP NOT NULL,
                    Processada NUMBER(1) DEFAULT 0 NOT NULL
                )
                """
            ];

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var comando in comandos)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = comando;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private ReceberTransacaoAppService CriarServico()
        {
            var configuracao = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OracleConnectionString"] = _connectionString
                })
                .Build();

            var unitOfWork = new UnitOfWork(configuracao);
            var clienteRepository = new ClienteRepository(unitOfWork);
            var transacaoRepository = new TransacaoRepository(unitOfWork);
            var outboxRepository = new OutboxRepository(unitOfWork);

            return new ReceberTransacaoAppService(unitOfWork, clienteRepository, transacaoRepository, outboxRepository);
        }

        [Fact]
        public async Task ReceberTransacao_ChamadoDuasVezesComMesmoTransacaoExternaId_NaoDeveDuplicar()
        {
            var request = new ReceberTransacaoRequest
            {
                Cpf = Cpf.Criar("12345678909"),
                Valor = Dinheiro.Criar(10.50m),
                PdvId = "PDV-INTEGRATION-TEST",
                TransacaoExternaId = Guid.NewGuid()
            };

            var primeiraChamada = await CriarServico().ReceberTransacao(request);
            var segundaChamada = await CriarServico().ReceberTransacao(request);

            Assert.Equal(primeiraChamada, segundaChamada);

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Transacoes WHERE TransacaoExternaId = :id";
            cmd.Parameters.Add(new OracleParameter("id", request.TransacaoExternaId.ToString()));
            var quantidade = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            Assert.Equal(1, quantidade);
        }
    }
}
