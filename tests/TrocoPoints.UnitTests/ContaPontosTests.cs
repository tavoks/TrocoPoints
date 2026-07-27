using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class ContaPontosTests
    {
        [Fact]
        public void CreditarPontos_DeveConverterValorParaPontosECreditar()
        {
            var conta = ContaPontos.Criar(clienteId: 1);

            var pontos = conta.CreditarPontos(Dinheiro.Criar(10.50m));

            Assert.Equal(105, pontos);
            Assert.Equal(105, conta.SaldoAtual);
        }

        [Fact]
        public void CreditarPontos_ChamadoDuasVezes_DeveAcumularSaldo()
        {
            var conta = ContaPontos.Criar(clienteId: 1);

            conta.CreditarPontos(Dinheiro.Criar(10.00m));
            conta.CreditarPontos(Dinheiro.Criar(5.00m));

            Assert.Equal(150, conta.SaldoAtual);
        }

        [Fact]
        public void CreditarPontos_ComValorQueNaoGeraPontos_DeveLancarExcecao()
        {
            var conta = ContaPontos.Criar(clienteId: 1);

            // Dinheiro exige > 0, mas um valor muito pequeno pode truncar para 0 pontos.
            var valorMinimo = Dinheiro.Criar(0.01m);

            Assert.Throws<InvalidOperationException>(() => conta.CreditarPontos(valorMinimo));
        }
    }
}
