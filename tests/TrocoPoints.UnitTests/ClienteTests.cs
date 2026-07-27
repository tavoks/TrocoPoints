using TrocoPoints.Domain.Models;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class ClienteTests
    {
        private static Cpf CpfValido() => Cpf.Criar("12345678909");

        [Fact]
        public void Criar_SemNome_DevePermitir()
        {
            var cliente = Cliente.Criar(nome: null, CpfValido());

            Assert.Null(cliente.Nome);
            Assert.Equal(0, cliente.Id);
        }

        [Fact]
        public void Criar_ComNomeVazio_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => Cliente.Criar(nome: "   ", CpfValido()));
        }

        [Fact]
        public void AtualizarNome_ComNomeVazio_DeveLancarExcecao()
        {
            var cliente = Cliente.Criar(nome: null, CpfValido());

            Assert.Throws<ArgumentException>(() => cliente.AtualizarNome(""));
        }

        [Fact]
        public void AtualizarNome_ComNomeValido_DeveAtualizar()
        {
            var cliente = Cliente.Criar(nome: null, CpfValido());

            cliente.AtualizarNome("Fulano");

            Assert.Equal("Fulano", cliente.Nome);
        }

        [Fact]
        public void Reconstituir_DeveManterIdOriginal()
        {
            var cliente = Cliente.Reconstituir(id: 42, nome: "Fulano", CpfValido());

            Assert.Equal(42, cliente.Id);
        }
    }
}
