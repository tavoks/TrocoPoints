namespace TrocoPoints.Application.Dtos.Eventos
{
    public class TransacaoRecebidaEvento
    {
        public int ClienteId { get; set; }
        public decimal Valor { get; set; }
        public string PdvId { get; set; } = string.Empty;
        public Guid TransacaoExternaId { get; set; }
        public DateTime DataHora { get; set; }
    }
}
