using TrocoPoints.Domain.Models;

namespace TrocoPoints.UnitTests
{
    public class PontosLedgerTests
    {
        [Fact]
        public void Criar_ComTransacaoExternaIdVazio_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => PontosLedger.Criar(clienteId: 1, Guid.Empty, pontos: 100));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Criar_ComPontosMenorOuIgualAZero_DeveLancarExcecao(int pontos)
        {
            Assert.Throws<ArgumentException>(() => PontosLedger.Criar(clienteId: 1, Guid.NewGuid(), pontos));
        }

        [Fact]
        public void Criar_ComDadosValidos_DeveCriarComIdZero()
        {
            var lancamento = PontosLedger.Criar(clienteId: 1, Guid.NewGuid(), pontos: 100);

            Assert.Equal(0, lancamento.Id);
            Assert.Equal(100, lancamento.Pontos);
        }
    }
}
