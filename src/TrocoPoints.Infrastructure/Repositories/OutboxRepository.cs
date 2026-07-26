using Dapper;
using TrocoPoints.Application.Dtos.TransacaoOutbox;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Infrastructure.Persistence;

namespace TrocoPoints.Infrastructure.Repositories
{
    public class OutboxRepository : RepositoryBase, IOutboxRepository
    {
        public OutboxRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task AdicionarAsync(TransacaoOutboxDTO transacaoOutbox, CancellationToken ct = default)
        {
            const string sql = "INSERT INTO OutboxMessages (Id, TipoEvento, Payload, DataCriacao, Processada) " +
                "VALUES (:Id, :TipoEvento, :Payload, :DataCriacao, :Processada)";

            var parametros = new
            {
                Id = transacaoOutbox.Id.ToString(),
                TipoEvento = transacaoOutbox.TipoEvento.ToString(),
                Payload = transacaoOutbox.Payload,
                DataCriacao = transacaoOutbox.DataCriacao,
                Processada = transacaoOutbox.Processada ? 1 : 0
            };

            var comando = new CommandDefinition(sql, parametros, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(comando);
        }
    }
}
