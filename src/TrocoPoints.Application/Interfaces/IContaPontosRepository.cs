using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Interfaces
{
    public interface IContaPontosRepository
    {
        Task<ContaPontos?> BuscarPorClienteIdAsync(int clienteId, CancellationToken ct = default);
        Task<int> AdicionarAsync(ContaPontos contaPontos, CancellationToken ct = default);
        Task AtualizarAsync(ContaPontos contaPontos, CancellationToken ct = default);
    }
}
