using System.Data;
using Dapper;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;
using TrocoPoints.Infrastructure.Persistence;

namespace TrocoPoints.Infrastructure.Repositories
{
    public class ContaPontosRepository : RepositoryBase, IContaPontosRepository
    {
        public ContaPontosRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<ContaPontos?> BuscarPorClienteIdAsync(int clienteId, CancellationToken ct = default)
        {
            const string sql = "SELECT Id, ClienteId, SaldoAtual FROM ContasPontos WHERE ClienteId = :ClienteId";

            var comando = new CommandDefinition(sql, new { ClienteId = clienteId }, Transaction, cancellationToken: ct);
            var linha = await Connection.QuerySingleOrDefaultAsync(comando);

            if (linha is null)
                return null;

            return ContaPontos.Reconstituir((int)linha.ID, (int)linha.CLIENTEID, (int)linha.SALDOATUAL);
        }

        public async Task<int> AdicionarAsync(ContaPontos contaPontos, CancellationToken ct = default)
        {
            var parametros = new DynamicParameters();
            parametros.Add("ClienteId", contaPontos.ClienteId);
            parametros.Add("SaldoAtual", contaPontos.SaldoAtual);
            parametros.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            const string sql = "INSERT INTO ContasPontos (ClienteId, SaldoAtual) VALUES (:ClienteId, :SaldoAtual) " +
                "RETURNING id INTO :Id";

            var comando = new CommandDefinition(sql, parametros, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(comando);

            return parametros.Get<int>("Id");
        }

        public async Task AtualizarAsync(ContaPontos contaPontos, CancellationToken ct = default)
        {
            const string sql = "UPDATE ContasPontos SET SaldoAtual = :SaldoAtual WHERE Id = :Id";

            var comando = new CommandDefinition(
                sql,
                new { SaldoAtual = contaPontos.SaldoAtual, Id = contaPontos.Id },
                Transaction,
                cancellationToken: ct);

            await Connection.ExecuteAsync(comando);
        }
    }
}
