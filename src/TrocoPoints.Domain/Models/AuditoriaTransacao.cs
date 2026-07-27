namespace TrocoPoints.Domain.Models
{
    public sealed class AuditoriaTransacao
    {
        public Guid TransacaoExternaId { get; private set; }
        public int ClienteId { get; private set; }
        public string PdvId { get; private set; }
        public decimal Valor { get; private set; }
        public int PontosCreditados { get; private set; }
        public DateTime DataTransacao { get; private set; }
        public DateTime DataProcessamento { get; private set; }

        private AuditoriaTransacao(
            Guid transacaoExternaId,
            int clienteId,
            string pdvId,
            decimal valor,
            int pontosCreditados,
            DateTime dataTransacao,
            DateTime dataProcessamento)
        {
            TransacaoExternaId = transacaoExternaId;
            ClienteId = clienteId;
            PdvId = pdvId;
            Valor = valor;
            PontosCreditados = pontosCreditados;
            DataTransacao = dataTransacao;
            DataProcessamento = dataProcessamento;
        }

        public static AuditoriaTransacao Criar(
            Guid transacaoExternaId,
            int clienteId,
            string pdvId,
            decimal valor,
            int pontosCreditados,
            DateTime dataTransacao)
        {
            if (transacaoExternaId == Guid.Empty)
                throw new ArgumentException("O identificador externo da transação não pode ser vazio.", nameof(transacaoExternaId));

            if (string.IsNullOrWhiteSpace(pdvId))
                throw new ArgumentException("O identificador do PDV não pode ser vazio.", nameof(pdvId));

            if (valor <= 0)
                throw new ArgumentException("O valor da transação deve ser maior que zero.", nameof(valor));

            if (pontosCreditados <= 0)
                throw new ArgumentException("A quantidade de pontos creditados deve ser maior que zero.", nameof(pontosCreditados));

            return new AuditoriaTransacao(transacaoExternaId, clienteId, pdvId, valor, pontosCreditados, dataTransacao, DateTime.UtcNow);
        }

        public static AuditoriaTransacao Reconstituir(
            Guid transacaoExternaId,
            int clienteId,
            string pdvId,
            decimal valor,
            int pontosCreditados,
            DateTime dataTransacao,
            DateTime dataProcessamento)
            => new AuditoriaTransacao(transacaoExternaId, clienteId, pdvId, valor, pontosCreditados, dataTransacao, dataProcessamento);
    }
}
