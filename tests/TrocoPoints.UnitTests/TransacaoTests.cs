using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class TransacaoTests
    {
        [Fact]
        public void Criar_ComPdvIdVazio_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() =>
                Transacao.Criar(clienteId: 1, Dinheiro.Criar(10), pdvId: "", Guid.NewGuid()));
        }

        [Fact]
        public void Criar_ComTransacaoExternaIdVazio_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() =>
                Transacao.Criar(clienteId: 1, Dinheiro.Criar(10), pdvId: "PDV-001", Guid.Empty));
        }

        [Fact]
        public void Criar_ComDadosValidos_DeveCriarComIdZeroEDataUtcNow()
        {
            var antes = DateTime.UtcNow;
            var transacao = Transacao.Criar(clienteId: 1, Dinheiro.Criar(10), pdvId: "PDV-001", Guid.NewGuid());
            var depois = DateTime.UtcNow;

            Assert.Equal(0, transacao.Id);
            Assert.InRange(transacao.DataHora, antes, depois);
        }
    }
}
