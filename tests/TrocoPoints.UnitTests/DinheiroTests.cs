using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class DinheiroTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10.50)]
        public void Criar_ComValorMenorOuIgualAZero_DeveLancarExcecao(decimal valor)
        {
            Assert.Throws<ArgumentException>(() => Dinheiro.Criar(valor));
        }

        [Fact]
        public void Criar_ComValorValido_DeveArredondarParaDuasCasas()
        {
            var dinheiro = Dinheiro.Criar(10.567m);

            Assert.Equal(10.57m, dinheiro.Valor);
        }

        [Fact]
        public void ComMesmoValor_DeveSerIgual()
        {
            var d1 = Dinheiro.Criar(10.50m);
            var d2 = Dinheiro.Criar(10.50m);

            Assert.Equal(d1, d2);
            Assert.Equal(d1.GetHashCode(), d2.GetHashCode());
        }
    }
}
