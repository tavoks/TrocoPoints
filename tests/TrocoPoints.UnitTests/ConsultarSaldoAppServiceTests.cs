using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Application.Services;
using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class ConsultarSaldoAppServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IClienteRepository> _clienteRepository = new();
        private readonly Mock<IContaPontosRepository> _contaPontosRepository = new();
        private readonly Mock<IDistributedCache> _cache = new();

        private ConsultarSaldoAppService CriarServico()
            => new(_unitOfWork.Object, _clienteRepository.Object, _contaPontosRepository.Object, _cache.Object);

        // GetStringAsync/SetStringAsync são extension methods por cima de GetAsync/SetAsync (byte[]) -
        // não dá pra mockar extension method direto, então mockamos o método real por baixo.

        [Fact]
        public async Task ConsultarSaldo_ComCpfDesconhecido_DeveDevolverNull()
        {
            var cpf = Cpf.Criar("12345678909");
            _clienteRepository.Setup(r => r.BuscarPorCpfAsync(cpf, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

            var resultado = await CriarServico().ConsultarSaldo(cpf);

            Assert.Null(resultado);
            _contaPontosRepository.Verify(r => r.BuscarPorClienteIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ConsultarSaldo_ComCacheHit_NaoDeveConsultarContaPontos()
        {
            var cpf = Cpf.Criar("12345678909");
            var cliente = Cliente.Reconstituir(id: 2, nome: null, cpf);
            _clienteRepository.Setup(r => r.BuscarPorCpfAsync(cpf, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
            _cache.Setup(c => c.GetAsync("saldo:cliente:2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("155"));

            var resultado = await CriarServico().ConsultarSaldo(cpf);

            Assert.Equal(155, resultado);
            _contaPontosRepository.Verify(r => r.BuscarPorClienteIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ConsultarSaldo_ComCacheMiss_DeveConsultarOracleEGravarNoCache()
        {
            var cpf = Cpf.Criar("12345678909");
            var cliente = Cliente.Reconstituir(id: 2, nome: null, cpf);
            var contaPontos = ContaPontos.Reconstituir(id: 1, clienteId: 2, saldoAtual: 105);

            _clienteRepository.Setup(r => r.BuscarPorCpfAsync(cpf, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
            _cache.Setup(c => c.GetAsync("saldo:cliente:2", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
            _contaPontosRepository.Setup(r => r.BuscarPorClienteIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(contaPontos);

            var resultado = await CriarServico().ConsultarSaldo(cpf);

            Assert.Equal(105, resultado);
            _cache.Verify(c => c.SetAsync(
                "saldo:cliente:2",
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "105"),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ConsultarSaldo_ComClienteSemContaPontos_DeveDevolverZero()
        {
            var cpf = Cpf.Criar("12345678909");
            var cliente = Cliente.Reconstituir(id: 2, nome: null, cpf);

            _clienteRepository.Setup(r => r.BuscarPorCpfAsync(cpf, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
            _cache.Setup(c => c.GetAsync("saldo:cliente:2", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
            _contaPontosRepository.Setup(r => r.BuscarPorClienteIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((ContaPontos?)null);

            var resultado = await CriarServico().ConsultarSaldo(cpf);

            Assert.Equal(0, resultado);
        }
    }
}
