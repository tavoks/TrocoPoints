using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Interfaces
{
    public interface ITransacaoRepository
    {
        Task<Transacao?> BuscarPorTransacaoExternaIdAsync(Guid transacaoExternaId, CancellationToken ct = default);
        Task<int> AdicionarAsync(Transacao transacao, CancellationToken ct = default);
    }
}
