namespace Taqueria.Domain.Tests;
using Taqueria.Domain.Entities;

public sealed class ProductoTests
{
    [Fact]
    public void ObtenerPrecioVigente_DevuelveElPrecioMasRecienteAplicable()
    {
        var producto = new Producto(1, "Taco Pastor", 1);
        producto.Precios.Add(new PrecioProducto(1, 20m, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        producto.Precios.Add(new PrecioProducto(1, 22m, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)));
        producto.Precios.Add(new PrecioProducto(1, 24m, new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)));

        var precio = producto.ObtenerPrecioVigente(new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(22m, precio.Precio);
    }
}
