using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Domain.Models
{
    public class Cliente
    {
        public int Id { get; private set; }
        public Cpf Cpf { get; private set; }
        public string? Nome { get; private set; }

        private Cliente(int id, Cpf cpf, string? nome)
        {
            Id = id;
            Cpf = cpf;
            Nome = nome;
        }

        public static Cliente Criar(string? nome, Cpf cpf)
        {
            if (nome is not null && !ValidarNome(nome))
                throw new ArgumentException("Nome Vazio");
            return new Cliente(id: 0, cpf, nome);
        }
            
        public static Cliente Reconstituir(int id, string? nome, Cpf cpf)
        {
            if (nome is not null && !ValidarNome(nome))
                throw new ArgumentException("Nome Vazio");
            return new Cliente(id, cpf, nome);
        }

        public void AtualizarNome(string? nome)
        {
            if (!ValidarNome(nome))
                throw new ArgumentException("Nome inválido");
            Nome = nome;
        }

        private static bool ValidarNome(string? nome)
        {
            return !string.IsNullOrWhiteSpace(nome);
        }
    }
}
