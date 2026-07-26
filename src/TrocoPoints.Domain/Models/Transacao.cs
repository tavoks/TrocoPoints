using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Domain.Models
{
    public sealed class Transacao
    {
        public int Id { get; private set; }
        public int ClienteId { get; private set; }
        public Dinheiro Dinheiro { get; private set; }
        public string PdvId { get; private set; }
        public Guid TransacaoExternaId { get; private set; }
        public DateTime DataHora { get; private set; }

        private Transacao(int id, int clienteId, Dinheiro dinheiro, string pdvId, Guid transacaoExternaId, DateTime dataHora)
        {
            Id = id;
            ClienteId = clienteId;
            Dinheiro = dinheiro;
            PdvId = pdvId;
            TransacaoExternaId = transacaoExternaId;
            DataHora = dataHora;
        }

        public static Transacao Criar(int clienteId, Dinheiro dinheiro, string pdvId, Guid transacaoExternaId)
        {
            if (string.IsNullOrWhiteSpace(pdvId))
                throw new ArgumentException("O identificador do PDV não pode ser vazio.", nameof(pdvId));

            if (transacaoExternaId == Guid.Empty)
                throw new ArgumentException("O identificador externo da transação não pode ser vazio.", nameof(transacaoExternaId));

            return new Transacao(id: 0, clienteId, dinheiro, pdvId, transacaoExternaId, DateTime.UtcNow);
        }

        public static Transacao Reconstituir(int id, int clienteId, Dinheiro dinheiro, string pdvId, Guid transacaoExternaId, DateTime dataHora)
            => new Transacao(id, clienteId, dinheiro, pdvId, transacaoExternaId, dataHora);
    }
}
