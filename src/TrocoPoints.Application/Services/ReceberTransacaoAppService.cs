using System.Text.Json;
using TrocoPoints.Application.Dtos.Requests;
using TrocoPoints.Application.Dtos.TransacaoOutbox;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;

namespace TrocoPoints.Application.Services
{
    public class ReceberTransacaoAppService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IOutboxRepository _outboxRepository;

        public ReceberTransacaoAppService(IUnitOfWork unitOfWork, IClienteRepository clienteRepository,
            ITransacaoRepository transacaoRepository, IOutboxRepository outboxRepository)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _transacaoRepository = transacaoRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task<int> ReceberTransacao(ReceberTransacaoRequest request, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.IniciarTransacaoAsync();

                var transacaoExistente = await _transacaoRepository.BuscarPorTransacaoExternaIdAsync(request.TransacaoExternaId, ct);
                if (transacaoExistente is not null)
                {
                    await _unitOfWork.RollbackAsync();
                    return transacaoExistente.Id;
                }

                var cliente = await BuscarOuCriarCliente(request, ct);

                var transacao = Transacao.Criar(cliente.Id, request.Valor, request.PdvId, request.TransacaoExternaId);
                var transacaoId = await _transacaoRepository.AdicionarAsync(transacao, ct);

                var payload = JsonSerializer.Serialize(new
                {
                    transacao.ClienteId,
                    Valor = transacao.Dinheiro.Valor,
                    transacao.PdvId,
                    transacao.TransacaoExternaId,
                    transacao.DataHora
                });
                var mensagemOutbox = new TransacaoOutboxDTO(TipoEventoOutboxEnum.TransacaoRecebida, payload);
                await _outboxRepository.AdicionarAsync(mensagemOutbox, ct);

                await _unitOfWork.CommitAsync();

                return transacaoId;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task<Cliente> BuscarOuCriarCliente(ReceberTransacaoRequest request, CancellationToken ct = default)
        {
            var cliente = await _clienteRepository.BuscarPorCpfAsync(request.Cpf, ct);
            if (cliente is not null)
                return cliente;

            var novoCliente = Cliente.Criar(nome: null, request.Cpf);
            var novoClienteId = await _clienteRepository.AdicionarAsync(novoCliente, ct);

            return Cliente.Reconstituir(novoClienteId, novoCliente.Nome, novoCliente.Cpf);
        }
    }
}
