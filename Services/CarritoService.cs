using System.Text.Json;
using HuellitasFelices.Models;
using Microsoft.AspNetCore.Http;

namespace HuellitasFelices.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "Carrito";

        public CarritoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        private List<CarritoItem> ObtenerDesdeSession()
        {
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(json) ?? new List<CarritoItem>();
        }

        private void GuardarEnSession(List<CarritoItem> items)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(items));
        }

        public List<CarritoItem> ObtenerItems() => ObtenerDesdeSession();

        public void Agregar(int productoId, string nombre, decimal precio, string? categoria, string? unidad, int stock, int cantidad = 1)
        {
            var items = ObtenerDesdeSession();
            var existente = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                if (existente.Cantidad > stock)
                    existente.Cantidad = stock;
            }
            else
            {
                items.Add(new CarritoItem
                {
                    ProductoId = productoId,
                    Nombre = nombre,
                    Precio = precio,
                    Cantidad = Math.Min(cantidad, stock),
                    CategoriaNombre = categoria,
                    UnidadMedida = unidad,
                    StockDisponible = stock
                });
            }

            GuardarEnSession(items);
        }

        public void Eliminar(int productoId)
        {
            var items = ObtenerDesdeSession();
            items.RemoveAll(i => i.ProductoId == productoId);
            GuardarEnSession(items);
        }

        public void ActualizarCantidad(int productoId, int cantidad)
        {
            var items = ObtenerDesdeSession();
            var item = items.FirstOrDefault(i => i.ProductoId == productoId);
            if (item != null)
            {
                if (cantidad <= 0)
                    items.Remove(item);
                else
                    item.Cantidad = Math.Min(cantidad, item.StockDisponible);
            }
            GuardarEnSession(items);
        }

        public void Vaciar()
        {
            Session.Remove(SessionKey);
        }

        public int ContarItems()
        {
            return ObtenerDesdeSession().Sum(i => i.Cantidad);
        }

        public decimal Total()
        {
            return ObtenerDesdeSession().Sum(i => i.Subtotal);
        }
    }
}
