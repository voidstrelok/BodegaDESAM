using BodegaDESAM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BodegaDESAM.Controllers
{
    [ApiController]
    [Authorize(Roles = BodegaDESAM.Auth.AppRoles.Public)]
    [Route("reports/entradas")]
    public class EntradaReportController : ControllerBase
    {
        private readonly EntradaService _entradaService;
        private readonly EntradaPdfReportService _pdf;
        private readonly UserDisplayNameService _userDisplayName;
        private readonly BodegaAuthorizationService _bodegas;

        public EntradaReportController(EntradaService entradaService, EntradaPdfReportService pdf, UserDisplayNameService userDisplayName, BodegaAuthorizationService bodegas)
        {
            _entradaService = entradaService;
            _pdf = pdf;
            _userDisplayName = userDisplayName;
            _bodegas = bodegas;
        }

        [HttpGet("{id:int}.pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var entrada = await _entradaService.GetByIdAsync(id);
            if (entrada == null)
                return NotFound();
            if (!await _bodegas.PuedeAccederAsync(User, entrada.id_bodega)) return Forbid();

            var registradoPor = await _userDisplayName.GetNombreCompletoAsync(entrada.IdUsuario);
            var bytes = _pdf.CreatePdf(entrada, registradoPor);
            return File(bytes, "application/pdf", $"entrada-{id}.pdf");
        }
    }
}
