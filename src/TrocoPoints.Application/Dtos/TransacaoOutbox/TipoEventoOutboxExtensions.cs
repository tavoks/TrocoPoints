namespace TrocoPoints.Application.Dtos.TransacaoOutbox
{
    public static class TipoEventoOutboxExtensions
    {
        public static string ParaRoutingKey(this TipoEventoOutboxEnum tipoEvento) => tipoEvento switch
        {
            TipoEventoOutboxEnum.TransacaoRecebida => "transacao.recebida",
            _ => throw new ArgumentOutOfRangeException(nameof(tipoEvento), tipoEvento, "Tipo de evento sem routing key definida.")
        };
    }
}
