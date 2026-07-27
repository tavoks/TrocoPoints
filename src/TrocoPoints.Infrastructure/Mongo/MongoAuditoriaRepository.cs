using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrocoPoints.Application.Interfaces;
using TrocoPoints.Domain.Models;

namespace TrocoPoints.Infrastructure.Mongo
{
    public class MongoAuditoriaRepository : IAuditoriaRepository
    {
        private readonly IMongoCollection<AuditoriaTransacaoDocument> _colecao;
        public MongoAuditoriaRepository(IOptions<MongoDbOptions> options)
        {
            var opcoes = options.Value;

            var client = new MongoClient(opcoes.ConnectionString);
            var database = client.GetDatabase(opcoes.DatabaseName);
            _colecao = database.GetCollection<AuditoriaTransacaoDocument>(opcoes.AuditoriaCollectionName);
        }

        public async Task AdicionarAsync(AuditoriaTransacao auditoria, CancellationToken ct = default)
        {
            var documento = new AuditoriaTransacaoDocument
            {
                TransacaoExternaId = auditoria.TransacaoExternaId.ToString(),
                ClienteId = auditoria.ClienteId,
                PdvId = auditoria.PdvId,
                Valor = auditoria.Valor,
                PontosCreditados = auditoria.PontosCreditados,
                DataTransacao = auditoria.DataTransacao,
                DataProcessamento = auditoria.DataProcessamento
            };

            await _colecao.InsertOneAsync(documento, cancellationToken: ct);
        }

        public async Task<AuditoriaTransacao?> BuscarPorTransacaoExternaIdAsync(Guid transacaoExternaId, CancellationToken ct = default)
        {
            var filtro = Builders<AuditoriaTransacaoDocument>.Filter.Eq(d => d.TransacaoExternaId, transacaoExternaId.ToString());
            var documento = await _colecao.Find(filtro).FirstOrDefaultAsync(ct);

            if (documento is null)
                return null;

            return AuditoriaTransacao.Reconstituir(
                Guid.Parse(documento.TransacaoExternaId),
                documento.ClienteId,
                documento.PdvId,
                documento.Valor,
                documento.PontosCreditados,
                documento.DataTransacao,
                documento.DataProcessamento);
        }
    }
}
