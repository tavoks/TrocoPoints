using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Application.Dtos.Requests
{
    public class ReceberTransacaoRequest
    {
        public Cpf Cpf { get; set; }
        public Dinheiro Valor { get; set; }
        public string PdvId { get; set; }
        public Guid TransacaoExternaId { get; set; }
    }
}
