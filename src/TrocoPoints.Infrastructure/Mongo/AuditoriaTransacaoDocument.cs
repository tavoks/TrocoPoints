using MongoDB.Bson.Serialization.Attributes;

namespace TrocoPoints.Infrastructure.Mongo
{
    public class AuditoriaTransacaoDocument
    {
        [BsonId]
        public string TransacaoExternaId { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string PdvId { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int PontosCreditados { get; set; }
        public DateTime DataTransacao { get; set; }
        public DateTime DataProcessamento { get; set; }
    }
}
