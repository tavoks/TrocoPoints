using Dapper;
using Oracle.ManagedDataAccess.Client;
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

        public async Task<IReadOnlyList<TransacaoOutboxDTO>> BuscarPendentesAsync(int limite = 50, CancellationToken ct = default)
        {
            const string sql = "SELECT Id, TipoEvento, Payload, DataCriacao, Processada " +
                "FROM OutboxMessages " +
                "WHERE Processada = 0 " +
                "ORDER BY DataCriacao ASC " +
                "FETCH FIRST :Limite ROWS ONLY";

            var comando = new CommandDefinition(sql, new { Limite = limite }, Transaction, cancellationToken: ct);
            var linhas = await Connection.QueryAsync(comando);

            var mensagens = new List<TransacaoOutboxDTO>();
            foreach (var linha in linhas)
            {
                mensagens.Add(TransacaoOutboxDTO.Reconstituir(
                    id: Guid.Parse((string)linha.ID),
                    tipoEvento: Enum.Parse<TipoEventoOutboxEnum>((string)linha.TIPOEVENTO),
                    payload: (string)linha.PAYLOAD,
                    dataCriacao: (DateTime)linha.DATACRIACAO,
                    processada: Convert.ToInt32(linha.PROCESSADA) == 1));
            }

            return mensagens;
        }

        public async Task MarcarComoProcessadasAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idsComoTexto = ids.Select(id => id.ToString());
            const string sql = "UPDATE OutboxMessages SET Processada = 1 WHERE Id IN :Ids AND Processada = 0";

            var comando = new CommandDefinition(sql, new { Ids = idsComoTexto }, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(comando);
        }
    }
}
