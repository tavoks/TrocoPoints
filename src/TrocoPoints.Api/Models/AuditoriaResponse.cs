namespace TrocoPoints.Api.Models
{
    public class AuditoriaResponse
    {
        public Guid TransacaoExternaId { get; set; }
        public int ClienteId { get; set; }
        public required string PdvId { get; set; }
        public decimal Valor { get; set; }
        public int PontosCreditados { get; set; }
        public DateTime DataTransacao { get; set; }
        public DateTime DataProcessamento { get; set; }
    }
}
