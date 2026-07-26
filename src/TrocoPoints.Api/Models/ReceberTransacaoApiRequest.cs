namespace TrocoPoints.Api.Models
{
    public class ReceberTransacaoApiRequest
    {
        public required string Cpf { get; set; }
        public decimal Valor { get; set; }
        public required string PdvId { get; set; }
        public Guid TransacaoExternaId { get; set; }
    }
}
