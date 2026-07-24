using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HuellitasFelices.Services;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Controllers;

[Authorize]
public class IAController : Controller
{
    private readonly IOllamaService _ollama;
    private readonly IContextProviderService _contextProvider;
    private readonly AiSettings _aiSettings;
    private readonly ILogger<IAController> _logger;

    public IAController(IOllamaService ollama, IContextProviderService contextProvider, IOptions<AiSettings> aiSettings, ILogger<IAController> logger)
    {
        _ollama = ollama;
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
Responde de forma clara, profesional y en español. Usa los datos de la clinica proporcionados para responder.
Si no tienes suficiente informacion, indicalo. No inventes datos.

Datos de la clinica: {contexto}

Pregunta del usuario: {peticion.Prompt}

Respuesta:";

            var respuesta = await _ollama.GenerarRespuestaAsync(promptCompleto);

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
