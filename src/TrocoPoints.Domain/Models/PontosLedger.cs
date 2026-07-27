namespace TrocoPoints.Domain.Models
{
    public sealed class PontosLedger
    {
        public int Id { get; private set; }
        public int ClienteId { get; private set; }
        public Guid TransacaoExternaId { get; private set; }
        public int Pontos { get; private set; }
        public DateTime DataCredito { get; private set; }

        private PontosLedger(int id, int clienteId, Guid transacaoExternaId, int pontos, DateTime dataCredito)
        {
            Id = id;
            ClienteId = clienteId;
            TransacaoExternaId = transacaoExternaId;
            Pontos = pontos;
            DataCredito = dataCredito;
        }

        public static PontosLedger Criar(int clienteId, Guid transacaoExternaId, int pontos)
        {
            if (transacaoExternaId == Guid.Empty)
                throw new ArgumentException("O identificador externo da transação não pode ser vazio.", nameof(transacaoExternaId));

            if (pontos <= 0)
                throw new ArgumentException("A quantidade de pontos deve ser maior que zero.", nameof(pontos));

            return new PontosLedger(id: 0, clienteId, transacaoExternaId, pontos, DateTime.UtcNow);
        }

        public static PontosLedger Reconstituir(int id, int clienteId, Guid transacaoExternaId, int pontos, DateTime dataCredito)
            => new PontosLedger(id, clienteId, transacaoExternaId, pontos, dataCredito);
    }
}
