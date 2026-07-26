using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Application.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> BuscarPorCpfAsync(Cpf cpf, CancellationToken ct = default);
        Task<int> AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    }
}
