using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.UnitTests
{
    public class CpfTests
    {
        [Fact]
        public void cpf_ComMesmoValor_DeveSerIgualEDeveTerMesmoHashCode()
        {
            // Arrange
            var cpf1 = Cpf.Criar("123.456.789-09");
            var cpf2 = Cpf.Criar("12345678909");
            // Act & Assert
            Assert.Equal(cpf1, cpf2);
            Assert.Equal(cpf1.GetHashCode(), cpf2.GetHashCode());

            var set = new HashSet<Cpf> { cpf1 };
            Assert.Contains(cpf2, set);
        }
    }
}
