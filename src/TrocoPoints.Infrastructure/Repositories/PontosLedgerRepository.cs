using Dapper;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;
using TrocoPoints.Infrastructure.Persistence;

namespace TrocoPoints.Infrastructure.Repositories
{
    public class PontosLedgerRepository : RepositoryBase, IPontosLedgerRepository
    {
        public PontosLedgerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<bool> ExisteAsync(Guid transacaoExternaId, CancellationToken ct = default)
        {
            const string sql = "SELECT COUNT(1) FROM PontosLedger WHERE TransacaoExternaId = :TransacaoExternaId";

            var comando = new CommandDefinition(
                sql,
                new { TransacaoExternaId = transacaoExternaId.ToString() },
                Transaction,
                cancellationToken: ct);

            var quantidade = await Connection.ExecuteScalarAsync<int>(comando);
            return quantidade > 0;
        }

        public async Task AdicionarAsync(PontosLedger pontosLedger, CancellationToken ct = default)
        {
            const string sql = "INSERT INTO PontosLedger (ClienteId, TransacaoExternaId, Pontos, DataCredito) " +
                "VALUES (:ClienteId, :TransacaoExternaId, :Pontos, :DataCredito)";

            var parametros = new
            {
                ClienteId = pontosLedger.ClienteId,
                TransacaoExternaId = pontosLedger.TransacaoExternaId.ToString(),
                Pontos = pontosLedger.Pontos,
                DataCredito = pontosLedger.DataCredito
            };

            var comando = new CommandDefinition(sql, parametros, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(comando);
        }
    }
}
