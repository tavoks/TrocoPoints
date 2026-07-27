using TrocoPoints.Application.Dtos.TransacaoOutbox;

namespace TrocoPoints.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AdicionarAsync(TransacaoOutboxDTO transacaoOutbox, CancellationToken ct = default);
        Task<IReadOnlyList<TransacaoOutboxDTO>> BuscarPendentesAsync(int limite = 50, CancellationToken ct = default);
        Task MarcarComoProcessadasAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
