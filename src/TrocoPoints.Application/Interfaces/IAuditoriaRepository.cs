using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task AdicionarAsync(AuditoriaTransacao auditoria, CancellationToken ct = default);
        Task<AuditoriaTransacao?> BuscarPorTransacaoExternaIdAsync(Guid transacaoExternaId, CancellationToken ct = default);
    }
}
