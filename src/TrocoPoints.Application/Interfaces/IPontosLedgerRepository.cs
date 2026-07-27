using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Interfaces
{
    public interface IPontosLedgerRepository
    {
        Task<bool> ExisteAsync(Guid transacaoExternaId, CancellationToken ct = default);
        Task AdicionarAsync(PontosLedger pontosLedger, CancellationToken ct = default);
    }
}
