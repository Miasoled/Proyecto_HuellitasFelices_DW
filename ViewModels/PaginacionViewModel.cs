namespace HuellitasFelices.ViewModels
{
    /// <summary>
    /// Contiene los datos necesarios para renderizar controles de paginación en las vistas.
    /// </summary>
    public class PaginacionViewModel
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int TamanioPagina { get; set; }
        public string? Busqueda { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
