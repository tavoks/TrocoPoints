using Moq;
using TrocoPoints.Application.Dtos.Requests;
using TrocoPoints.Application.Dtos.TransacaoOutbox;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Application.Services;
using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class ReceberTransacaoAppServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IClienteRepository> _clienteRepository = new();
        private readonly Mock<ITransacaoRepository> _transacaoRepository = new();
        private readonly Mock<IOutboxRepository> _outboxRepository = new();

        private ReceberTransacaoAppService CriarServico()
            => new(_unitOfWork.Object, _clienteRepository.Object, _transacaoRepository.Object, _outboxRepository.Object);

        private static ReceberTransacaoRequest CriarRequest(Guid? transacaoExternaId = null) => new()
        {
            Cpf = Cpf.Criar("12345678909"),
            Valor = Dinheiro.Criar(10.50m),
            PdvId = "PDV-001",
            TransacaoExternaId = transacaoExternaId ?? Guid.NewGuid()
        };

        [Fact]
        public async Task ReceberTransacao_ComTransacaoJaExistente_DeveSerIdempotenteENaoCriarNadaNovo()
        {
            var request = CriarRequest();
            var transacaoExistente = Transacao.Reconstituir(
                id: 99, clienteId: 1, Dinheiro.Criar(10.50m), request.PdvId, request.TransacaoExternaId, DateTime.UtcNow);

            _transacaoRepository
                .Setup(r => r.BuscarPorTransacaoExternaIdAsync(request.TransacaoExternaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transacaoExistente);

            var resultado = await CriarServico().ReceberTransacao(request);

            Assert.Equal(99, resultado);
            _unitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _clienteRepository.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
            _transacaoRepository.Verify(r => r.AdicionarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Never);
            _outboxRepository.Verify(r => r.AdicionarAsync(It.IsAny<TransacaoOutboxDTO>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReceberTransacao_ComClienteNovo_DeveCriarClienteTransacaoEOutboxECommitar()
        {
            var request = CriarRequest();

            _transacaoRepository
                .Setup(r => r.BuscarPorTransacaoExternaIdAsync(request.TransacaoExternaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transacao?)null);

            _clienteRepository
                .Setup(r => r.BuscarPorCpfAsync(request.Cpf, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);

            _clienteRepository
                .Setup(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(7);

            _transacaoRepository
                .Setup(r => r.AdicionarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(55);

            var resultado = await CriarServico().ReceberTransacao(request);

            Assert.Equal(55, resultado);
            _clienteRepository.Verify(r => r.AdicionarAsync(It.Is<Cliente>(c => c.Cpf.Equals(request.Cpf)), It.IsAny<CancellationToken>()), Times.Once);
            _outboxRepository.Verify(r => r.AdicionarAsync(It.IsAny<TransacaoOutboxDTO>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _unitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task ReceberTransacao_ComClienteExistente_NaoDeveCriarClienteNovo()
        {
            var request = CriarRequest();
            var clienteExistente = Cliente.Reconstituir(id: 3, nome: "Fulano", request.Cpf);

            _transacaoRepository
                .Setup(r => r.BuscarPorTransacaoExternaIdAsync(request.TransacaoExternaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transacao?)null);

            _clienteRepository
                .Setup(r => r.BuscarPorCpfAsync(request.Cpf, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clienteExistente);

            _transacaoRepository
                .Setup(r => r.AdicionarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            await CriarServico().ReceberTransacao(request);

            _clienteRepository.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
            _transacaoRepository.Verify(r => r.AdicionarAsync(
                It.Is<Transacao>(t => t.ClienteId == clienteExistente.Id), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReceberTransacao_QuandoRepositorioFalha_DeveFazerRollbackEPropagarExcecao()
        {
            var request = CriarRequest();

            _transacaoRepository
                .Setup(r => r.BuscarPorTransacaoExternaIdAsync(request.TransacaoExternaId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Falha simulada de banco"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => CriarServico().ReceberTransacao(request));

            _unitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
