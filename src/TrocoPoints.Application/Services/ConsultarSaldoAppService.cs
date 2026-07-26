using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Application.Services
{
    public class ConsultarSaldoAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly IContaPontosRepository _contaPontosRepository;

        public ConsultarSaldoAppService(
            IUnitOfWork unitOfWork,
            IClienteRepository clienteRepository,
            IContaPontosRepository contaPontosRepository)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _contaPontosRepository = contaPontosRepository;
        }

        public async Task<int?> ConsultarSaldo(Cpf cpf, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.IniciarTransacaoAsync();

                var cliente = await _clienteRepository.BuscarPorCpfAsync(cpf, ct);
                if (cliente is null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                var contaPontos = await _contaPontosRepository.BuscarPorClienteIdAsync(cliente.Id, ct);

                await _unitOfWork.RollbackAsync();

                return contaPontos?.SaldoAtual ?? 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
