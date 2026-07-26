namespace TrocoPoints.Application.Dtos.TransacaoOutbox
{
    public class TransacaoOutboxDTO
    {
        public Guid Id { get; }
        public TipoEventoOutboxEnum TipoEvento { get; }
        public string Payload { get; }
        public DateTime DataCriacao { get; }
        public bool Processada { get; private set; }

        public TransacaoOutboxDTO(TipoEventoOutboxEnum tipoEvento, string payload)
        {
            if (!ValidarTransacao(payload))
            {
                throw new ArgumentException("Dados da transação vazios.");
            }

            Id = Guid.NewGuid();
            TipoEvento = tipoEvento;
            Payload = payload;
            DataCriacao = DateTime.UtcNow;
            Processada = false;
        }

        public void MarcarComoProcessada()
        {
            Processada = true;
        }

        private static bool ValidarTransacao(string payload)
        {
            return !string.IsNullOrEmpty(payload);
        }
    }
}