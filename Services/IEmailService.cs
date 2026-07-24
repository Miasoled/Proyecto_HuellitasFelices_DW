namespace HuellitasFelices.Services
{
    public interface IEmailService
    {
        Task EnviarConfirmacionCorreoAsync(string email, string nombreUsuario, string confirmationLink);
        Task EnviarRecuperarPasswordAsync(string email, string resetLink);
        Task EnviarCambioPasswordAsync(string email, string nombreUsuario);
        Task EnviarCuentaBloqueadaAsync(string email, string nombreUsuario);
        Task EnviarActivacionMFAAsync(string email, string nombreUsuario);
        Task EnviarVentaAprobadaAsync(string email, string nombreUsuario, int ventaId, decimal total, string detalle);
        Task EnviarPagoFallidoAsync(string email, string nombreUsuario, string razon);
        Task EnviarInventarioCriticoAsync(string email, string nombreProducto, int stockActual, int stockMinimo);
        Task EnviarAdopcionAsync(string email, string nombreSolicitante, string nombreAnimal, string especie, string codigo);
    }
}
