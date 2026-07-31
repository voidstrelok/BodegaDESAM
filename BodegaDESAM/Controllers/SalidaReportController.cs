using BodegaDESAM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BodegaDESAM.Controllers
{
    [ApiController]
    [Authorize(Roles = BodegaDESAM.Auth.AppRoles.Public)]
    [Route("reports/salidas")]
    public class SalidaReportController : ControllerBase
    {
        private readonly SalidaService _salidaService;
        private readonly SalidaPdfReportService _pdf;
        private readonly UserDisplayNameService _userDisplayName;

        public SalidaReportController(
            SalidaService salidaService,
            SalidaPdfReportService pdf,
            UserDisplayNameService userDisplayName)
        {
            _salidaService = salidaService;
            _pdf = pdf;
            _userDisplayName = userDisplayName;
        }

        [HttpGet("{id:int}.pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var salida = await _salidaService.GetByIdAsync(id);
            if (salida == null)
                return NotFound();

            var nombreQuienEntrega = await _userDisplayName.GetNombreCompletoAsync(salida.IdUsuario);
            var bytes = _pdf.CreatePdf(salida, nombreQuienEntrega);
            return File(bytes, "application/pdf", $"salida-{id}.pdf");
        }
    }
}
