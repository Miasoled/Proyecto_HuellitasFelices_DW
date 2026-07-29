using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HuellitasFelices.Services;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Controllers;

[Authorize]
public class IAController : Controller
{
    private readonly IAIService _ai;
    private readonly IContextProviderService _contextProvider;
    private readonly AiSettings _aiSettings;
    private readonly ILogger<IAController> _logger;
    private readonly IAuditService _auditService;

    public IAController(IAIService ai, IContextProviderService contextProvider, IOptions<AiSettings> aiSettings, ILogger<IAController> logger, IAuditService auditService)
    {
        _ai = ai;
        _contextProvider = contextProvider;
        _aiSettings = aiSettings.Value;
        _logger = logger;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Asistente()
    {
        ViewBag.ModelName = _aiSettings.ModelName;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Generar([FromBody] PeticionIA peticion)
    {
        if (string.IsNullOrWhiteSpace(peticion?.Prompt))
            return BadRequest(new { error = "La pregunta no puede estar vacia." });

        try
        {
            var contexto = await _contextProvider.ObtenerContextoAsync(peticion.Prompt);

            var promptCompleto = $@"Eres el asistente de la clinica Huellitas Felices. Puedes responder sobre salud de mascotas Y sobre datos de la clinica (ventas, inventario, consultas, clientes, etc). Si el contexto incluye datos de la clinica, USALOS para responder directamente. Nunca digas que no puedes o que no tienes informacion.

Para preguntas de SINTOMAS de mascota, responde con este formato:

Posibles causas: puede ser por alergia, hongos, estres o problemas hormonales.
Nivel de urgencia: bajo.
Que hacer ahora: revise si hay rascado, manchas rojas o cambios en la alimentacion. Mantenga la higiene del pelaje y observe por unos dias.
Señales de alarma: si hay perdida de pelo en zonas grandes, piel irritada o si no come en 24 horas, acuda al veterinario.
Este analisis es orientativo. Para un diagnostico definitivo, acuda a su veterinario.

Para preguntas sobre la CLINICA (ventas, inventario, mascotas, consultas, etc), responde directamente con los datos que te da el contexto. Ejemplo: 'Ultimas ventas: el 15/07 se vendio Royal Canin x2 por $45.00'.

REGLAS:
- Si el contexto contiene datos de la clinica, RESPONDE con esos datos. NUNCA digas que no puedes proporcionar informacion.
- NUNCA hagas preguntas al usuario.
- Solo pon el disclaimer del veterinario cuando la pregunta sea sobre salud o sintomas.
- Escribe en texto plano. Sin asteriscos ni negritas.

Contexto: {contexto}
Pregunta: {peticion.Prompt}
Respuesta:";

            var respuesta = await _ai.GenerateAsync(promptCompleto);

            if (string.IsNullOrEmpty(respuesta))
            {
                return Ok(new { respuesta = "No pude generar una respuesta. Verifique que Ollama este corriendo." });
            }

            await _auditService.LogAsync("EjecucionIA", "IA", null,
                usuarioEmail: User.Identity?.Name,
                direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                valorNuevo: $"Prompt: {peticion.Prompt.Substring(0, Math.Min(peticion.Prompt.Length, 200))}");

            return Ok(new { respuesta, contexto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar peticion IA");
            return Ok(new { respuesta = "Ocurrio un error al procesar su consulta. Intente de nuevo." });
        }
    }

    public class PeticionIA
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
