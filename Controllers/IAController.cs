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

    public IAController(IAIService ai, IContextProviderService contextProvider, IOptions<AiSettings> aiSettings, ILogger<IAController> logger)
    {
        _ai = ai;
        _contextProvider = contextProvider;
        _aiSettings = aiSettings.Value;
        _logger = logger;
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

            var promptCompleto = $@"Eres el asistente inteligente de la clinica veterinaria Huellitas Felices.
Responde de forma clara, profesional y en español. Sé conciso y útil.

INSTRUCCIONES:
- Si el contexto incluye datos de la clinica, úsalos para responder.
- Si el contexto incluye CUIDADO GENERAL, usa esa información para dar consejos prácticos y accionables.
- Si el contexto indica que no hay datos, responde con un mensaje amable y sugiere consultar al veterinario.
- Siempre termina recomendando acudir al veterinario para un diagnóstico profesional.
- No inventes nombres de medicamentos ni dosis. No uses información que no esté en el contexto.

Contexto disponible: {contexto}

Pregunta del usuario: {peticion.Prompt}

Respuesta:";

            var respuesta = await _ai.GenerateAsync(promptCompleto);

            if (string.IsNullOrEmpty(respuesta))
            {
                return Ok(new { respuesta = "No pude generar una respuesta. Verifique que Ollama este corriendo." });
            }

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
