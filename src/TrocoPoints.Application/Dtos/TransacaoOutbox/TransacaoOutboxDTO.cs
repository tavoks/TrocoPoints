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

        private TransacaoOutboxDTO(Guid id, TipoEventoOutboxEnum tipoEvento, string payload, DateTime dataCriacao, bool processada)
        {
            Id = id;
            TipoEvento = tipoEvento;
            Payload = payload;
            DataCriacao = dataCriacao;
            Processada = processada;
        }

        public void MarcarComoProcessada()
        {
            Processada = true;
        }

        private static bool ValidarTransacao(string payload)
        {
            return !string.IsNullOrEmpty(payload);
        }

        public static TransacaoOutboxDTO Reconstituir(Guid id, TipoEventoOutboxEnum tipoEvento, string payload, DateTime dataCriacao, bool processada)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id vazio.");

            return new TransacaoOutboxDTO(id, tipoEvento, payload, dataCriacao, processada);
        }
    }
}
