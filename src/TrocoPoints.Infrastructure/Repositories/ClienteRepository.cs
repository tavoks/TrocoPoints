using System.Data;
using Dapper;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;
using TrocoPoints.Infrastructure.Persistence;

namespace TrocoPoints.Infrastructure.Repositories
{
    public class ClienteRepository : RepositoryBase, IClienteRepository
    {
        public ClienteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<int> AdicionarAsync(Cliente cliente, CancellationToken ct = default)
        {
            var parametros = new DynamicParameters();
            parametros.Add("cpf", cliente.Cpf.Valor);
            parametros.Add("nome", cliente.Nome);
            parametros.Add("id", dbType: DbType.Int32, direction: ParameterDirection.Output);

            const string sql = "INSERT INTO clientes (cpf, nome) VALUES (:cpf, :nome) RETURNING id INTO :id"; 

            var command = new CommandDefinition(sql, parametros, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(command);

            return parametros.Get<int>("id");
        }

        public async Task<Cliente?> BuscarPorCpfAsync(Cpf cpf, CancellationToken ct = default)
        {
            const string sql = "SELECT id, cpf, nome FROM clientes WHERE cpf = :cpf";

            var command = new CommandDefinition(sql, new { cpf = cpf.Valor }, Transaction, cancellationToken: ct);
            var linha = await Connection.QuerySingleOrDefaultAsync(command);

            if (linha is null)
                return null;

            return Cliente.Reconstituir((int)linha.ID, (string?)linha.NOME, Cpf.Criar((string)linha.CPF));
        }
    }
}
