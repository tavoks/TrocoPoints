using Microsoft.Extensions.Caching.Distributed;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Application.Services
{
    public class ConsultarSaldoAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly IContaPontosRepository _contaPontosRepository;
        private readonly IDistributedCache _cache;

        public ConsultarSaldoAppService(
            IUnitOfWork unitOfWork,
            IClienteRepository clienteRepository,
            IContaPontosRepository contaPontosRepository,
            IDistributedCache distributedCache)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _contaPontosRepository = contaPontosRepository;
            _cache = distributedCache;
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

                var chave = $"saldo:cliente:{cliente.Id}";
                var valorCacheado = await _cache.GetStringAsync(chave, ct);

                if (valorCacheado is not null)
                {
                    await _unitOfWork.RollbackAsync();
                    return int.Parse(valorCacheado);
                }

                var contaPontos = await _contaPontosRepository.BuscarPorClienteIdAsync(cliente.Id, ct);
                var saldo = contaPontos?.SaldoAtual ?? 0;

                await _cache.SetStringAsync(
                    chave,
                    saldo.ToString(),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
                    ct);

                await _unitOfWork.RollbackAsync();

                return saldo;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
