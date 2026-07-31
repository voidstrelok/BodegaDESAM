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

        public EntradaReportController(EntradaService entradaService, EntradaPdfReportService pdf)
        {
            _entradaService = entradaService;
            _pdf = pdf;
        }

        [HttpGet("{id:int}.pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var entrada = await _entradaService.GetByIdAsync(id);
            if (entrada == null)
                return NotFound();

            var bytes = _pdf.CreatePdf(entrada);
            return File(bytes, "application/pdf", $"entrada-{id}.pdf");
        }
    }
}
