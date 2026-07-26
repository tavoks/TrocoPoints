using System.Text.RegularExpressions;

namespace TrocoPoints.Domain.ValueObjects
{
    public sealed class Cpf
    {
        public string Valor { get; }

        private Cpf(string valor)
        {
            Valor = valor;
        }

        public static Cpf Criar(string valor)
        {
            var validacao = ValidarCpf(valor);
            if (!string.IsNullOrWhiteSpace(validacao.Erros))
                throw new ArgumentException(validacao.Erros, nameof(valor));

            return new Cpf(validacao.ValorNormalizado);
        }

        private static (string ValorNormalizado, string Erros) ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return (cpf, "O CPF não pode ser nulo ou vazio.");

            var erros = new List<string>();

            cpf = Regex.Replace(cpf, "[^0-9]", "");
            
            if (cpf.Length is not 11)
                erros.Add($"O CPF {cpf} deve ter 11 caracteres.");

            string errosTratados = string.Join("\n", erros);
            return (cpf, errosTratados);
        }

        public override bool Equals(object? obj) 
            => obj is Cpf outro && Valor == outro.Valor;
        public override int GetHashCode() => Valor.GetHashCode();
        public override string ToString() => Valor;
    }
}
