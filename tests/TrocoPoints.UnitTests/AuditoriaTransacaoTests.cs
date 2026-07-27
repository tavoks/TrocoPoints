using TrocoPoints.Domain.Models;

namespace TrocoPoints.UnitTests
{
    public class AuditoriaTransacaoTests
    {
        [Fact]
        public void Criar_ComPdvIdVazio_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() =>
                AuditoriaTransacao.Criar(Guid.NewGuid(), clienteId: 1, pdvId: "", valor: 10, pontosCreditados: 100, DateTime.UtcNow));
        }

        [Fact]
        public void Criar_ComValorZero_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() =>
                AuditoriaTransacao.Criar(Guid.NewGuid(), clienteId: 1, pdvId: "PDV-001", valor: 0, pontosCreditados: 100, DateTime.UtcNow));
        }

        [Fact]
        public void Criar_ComDadosValidos_DeveCriarComDataProcessamentoUtcNow()
        {
            var antes = DateTime.UtcNow;
            var auditoria = AuditoriaTransacao.Criar(Guid.NewGuid(), clienteId: 1, pdvId: "PDV-001", valor: 10, pontosCreditados: 100, DateTime.UtcNow);
            var depois = DateTime.UtcNow;

            Assert.InRange(auditoria.DataProcessamento, antes, depois);
        }
    }
}
