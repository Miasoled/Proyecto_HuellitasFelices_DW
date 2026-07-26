namespace HuellitasFelices.Models;

public static class FotoUrlHelper
{
    // Normaliza rutas ya guardadas en la base, sin modificar su valor.
    public static string? Normalizar(string? fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl)) return null;

        var ruta = fotoUrl.Trim().Replace('\\', '/');
        if (ruta.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return ruta;

        ruta = ruta.TrimStart('~');
        if (!ruta.StartsWith('/'))
            ruta = ruta.Contains('/') ? $"/{ruta}" : $"/images/animales/{ruta}";

        return ruta;
    }
}
