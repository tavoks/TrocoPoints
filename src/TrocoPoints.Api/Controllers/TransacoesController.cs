using Microsoft.AspNetCore.Mvc;
using TrocoPoints.Api.Models;
using TrocoPoints.Application.Dtos.Requests;
using TrocoPoints.Application.Services;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Api.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    public class TransacoesController : ControllerBase
    {
        private readonly ReceberTransacaoAppService _receberTransacaoAppService;

        public TransacoesController(ReceberTransacaoAppService receberTransacaoAppService)
        {
            _receberTransacaoAppService = receberTransacaoAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReceberTransacaoApiRequest request, CancellationToken ct)
        {
            try
            {
                var cpf = Cpf.Criar(request.Cpf);
                var valor = Dinheiro.Criar(request.Valor);

                var appRequest = new ReceberTransacaoRequest
                {
                    Cpf = cpf,
                    Valor = valor,
                    PdvId = request.PdvId,
                    TransacaoExternaId = request.TransacaoExternaId
                };

                var transacaoId = await _receberTransacaoAppService.ReceberTransacao(appRequest, ct);

                return CreatedAtAction(nameof(Post), new { id = transacaoId }, new TransacaoResponse { Id = transacaoId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErroResponse { Mensagem = ex.Message });
            }
        }
    }
}
