using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Domain.Models
{
    public sealed class ContaPontos
    {
        private const int PontosPorReal = 10;

        public int Id { get; private set; }
        public int ClienteId { get; private set; }
        public int SaldoAtual { get; private set; }

        private ContaPontos(int id, int clienteId, int saldoAtual)
        {
            Id = id;
            ClienteId = clienteId;
            SaldoAtual = saldoAtual;
        }

        public static ContaPontos Criar(int clienteId)
            => new ContaPontos(id: 0, clienteId, saldoAtual: 0);

        public static ContaPontos Reconstituir(int id, int clienteId, int saldoAtual)
            => new ContaPontos(id, clienteId, saldoAtual);

        public int CreditarPontos(Dinheiro valorTransacao)
        {
            var pontos = (int)(valorTransacao.Valor * PontosPorReal);

            if (pontos <= 0)
                throw new InvalidOperationException("O valor da transação não gerou pontos suficientes para crédito.");

            SaldoAtual += pontos;
            return pontos;
        }
    }
}
