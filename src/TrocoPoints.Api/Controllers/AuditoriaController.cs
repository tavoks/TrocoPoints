using Microsoft.AspNetCore.Mvc;
using TrocoPoints.Api.Models;
using TrocoPoints.Application.Services;

namespace TrocoPoints.Api.Controllers
{
    [ApiController]
    [Route("api/auditoria")]
    public class AuditoriaController : ControllerBase
    {
        private readonly ConsultarAuditoriaAppService _consultarAuditoriaAppService;

        public AuditoriaController(ConsultarAuditoriaAppService consultarAuditoriaAppService)
        {
            _consultarAuditoriaAppService = consultarAuditoriaAppService;
        }

        [HttpGet("{transacaoExternaId}")]
        public async Task<IActionResult> Get(Guid transacaoExternaId, CancellationToken ct)
        {
            var auditoria = await _consultarAuditoriaAppService.ConsultarAsync(transacaoExternaId, ct);

            if (auditoria is null)
                return NotFound(new ErroResponse { Mensagem = "Registro de auditoria não encontrado." });

            return Ok(new AuditoriaResponse
            {
                TransacaoExternaId = auditoria.TransacaoExternaId,
                ClienteId = auditoria.ClienteId,
                PdvId = auditoria.PdvId,
                Valor = auditoria.Valor,
                PontosCreditados = auditoria.PontosCreditados,
                DataTransacao = auditoria.DataTransacao,
                DataProcessamento = auditoria.DataProcessamento
            });
        }
    }
}
