using System.Data;
using Dapper;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;
using TrocoPoints.Infrastructure.Persistence;

namespace TrocoPoints.Infrastructure.Repositories
{
    public class TransacaoRepository : RepositoryBase, ITransacaoRepository
    {
        public TransacaoRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<int> AdicionarAsync(Transacao transacao, CancellationToken ct = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("ClienteId", transacao.ClienteId);
            parameters.Add("Dinheiro", transacao.Dinheiro.Valor);
            parameters.Add("PdvId", transacao.PdvId);
            parameters.Add("TransacaoExternaId", transacao.TransacaoExternaId.ToString());
            parameters.Add("DataHora", transacao.DataHora);
            parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            const string sql = "INSERT INTO Transacoes (ClienteId, Dinheiro, PdvId, TransacaoExternaId, " +
                "DataHora) VALUES (:ClienteId, :Dinheiro, :PdvId, :TransacaoExternaId, :DataHora)" +
                " RETURNING id INTO :id";

            var command = new CommandDefinition(sql, parameters, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(command);

            return parameters.Get<int>("Id");
        }

        public async Task<Transacao?> BuscarPorTransacaoExternaIdAsync(Guid transacaoExternaId, CancellationToken ct = default)
        {
            const string sql = "SELECT Id, ClienteId, Dinheiro, PdvId, TransacaoExternaId, DataHora " +
                "FROM Transacoes WHERE TransacaoExternaId = :TransacaoExternaId";

            var command = new CommandDefinition(sql, new { TransacaoExternaId = transacaoExternaId.ToString() }, 
                Transaction, cancellationToken: ct);
            var linha = await Connection.QuerySingleOrDefaultAsync(command);
            
            if(linha is null)
                return null;

            return Transacao.Reconstituir((int)linha.ID, (int)linha.CLIENTEID,
                Dinheiro.Criar((decimal)linha.DINHEIRO), (string)linha.PDVID,
                Guid.Parse((string)linha.TRANSACAOEXTERNAID), (DateTime)linha.DATAHORA);
        }
    }
}
