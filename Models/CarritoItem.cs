namespace HuellitasFelices.Models
{
    public class CarritoItem
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? UnidadMedida { get; set; }
        public int StockDisponible { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
