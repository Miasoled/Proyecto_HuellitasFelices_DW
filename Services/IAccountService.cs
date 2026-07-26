using HuellitasFelices.Areas.Identity.Pages.Account;
using HuellitasFelices.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace HuellitasFelices.Services;

public interface IAccountService
{
    Task<PanelClienteDto?> ObtenerPanelClienteAsync(string email);
    Task<PanelDoctorDto?> ObtenerPanelDoctorAsync(string email);
    Task<List<UsuarioConRolesDto>> ObtenerUsuariosAsync();
    Task<(bool Exito, IEnumerable<string> Errores)> RegistrarUsuarioInternoAsync(RegisterInternoViewModel model);
    Task<(bool Exito, IEnumerable<string> Errores)> CambiarPasswordAsync(IdentityUser user, string passwordActual, string nuevaPassword);
}
