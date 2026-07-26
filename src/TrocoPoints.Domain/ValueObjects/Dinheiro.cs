namespace TrocoPoints.Domain.ValueObjects
{
    public sealed class Dinheiro
    {
        public decimal Valor { get; }

        private Dinheiro(decimal valor)
        {
            Valor = valor;
        }

        public static Dinheiro Criar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.", nameof(valor));

            var valorArredondado = Math.Round(valor, 2, MidpointRounding.ToEven);

            return new Dinheiro(valorArredondado);
        }

        public override bool Equals(object? obj)
            => obj is Dinheiro outro && Valor == outro.Valor;

        public override int GetHashCode() => Valor.GetHashCode();

        public override string ToString() => Valor.ToString("F2");
    }
}
