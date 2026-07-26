using TrocoPoints.Application.Dtos.TransacaoOutbox;

namespace TrocoPoints.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AdicionarAsync(TransacaoOutboxDTO transacaoOutbox, CancellationToken ct = default);
    }
}
