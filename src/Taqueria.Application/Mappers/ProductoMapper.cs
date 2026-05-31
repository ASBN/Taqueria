namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Productos;
using Taqueria.Domain.Entities;

public static class ProductoMapper
{
    public static ProductoDto ToDto(this Producto entidad, DateTime fechaConsultaUtc)
    {
        var precioVigente = entidad.Precios
            .Where(precio => precio.VigenteDesdeUtc <= fechaConsultaUtc)
            .OrderByDescending(precio => precio.VigenteDesdeUtc)
            .FirstOrDefault();

        return new ProductoDto(
            entidad.Id,
            entidad.CategoriaProductoId,
            entidad.CategoriaProducto?.Nombre ?? string.Empty,
            entidad.Nombre,
            entidad.OrdenVisual,
            entidad.Activo,
            precioVigente?.Precio);
    }
}
