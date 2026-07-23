namespace HuellitasFelices.Services
{
    public interface ICarritoService
    {
        List<Models.CarritoItem> ObtenerItems();
        void Agregar(int productoId, string nombre, decimal precio, string? categoria, string? unidad, int stock, int cantidad = 1);
        void Eliminar(int productoId);
        void ActualizarCantidad(int productoId, int cantidad);
        void Vaciar();
        int ContarItems();
        decimal Total();
    }
}
