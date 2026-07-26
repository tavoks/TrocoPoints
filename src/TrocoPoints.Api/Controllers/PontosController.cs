using Microsoft.AspNetCore.Mvc;
using TrocoPoints.Api.Models;
using TrocoPoints.Application.Services;
using TrocoPoints.Domain.ValueObjects;

namespace TrocoPoints.Api.Controllers
{
    [ApiController]
    [Route("api/pontos")]
    public class PontosController : ControllerBase
    {
        private readonly ConsultarSaldoAppService _consultarSaldoAppService;

        public PontosController(ConsultarSaldoAppService consultarSaldoAppService)
        {
            _consultarSaldoAppService = consultarSaldoAppService;
        }

        [HttpGet("{cpf}")]
        public async Task<IActionResult> Get(string cpf, CancellationToken ct)
        {
            try
            {
                var cpfVo = Cpf.Criar(cpf);
                var saldo = await _consultarSaldoAppService.ConsultarSaldo(cpfVo, ct);

                if (saldo is null)
                    return NotFound(new ErroResponse { Mensagem = "Cliente não encontrado." });

                return Ok(new SaldoResponse { Cpf = cpfVo.Valor, Saldo = saldo.Value });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErroResponse { Mensagem = ex.Message });
            }
        }
    }
}
