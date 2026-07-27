using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Services
{
    public class ConsultarAuditoriaAppService
    {
        private readonly IAuditoriaRepository _auditoriaRepository;

        public ConsultarAuditoriaAppService(IAuditoriaRepository auditoriaRepository)
        {
            _auditoriaRepository = auditoriaRepository;
        }

        public Task<AuditoriaTransacao?> ConsultarAsync(Guid transacaoExternaId, CancellationToken ct = default)
            => _auditoriaRepository.BuscarPorTransacaoExternaIdAsync(transacaoExternaId, ct);
    }
}
