namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.PreciosProducto;
using Taqueria.Domain.Entities;

public static class PrecioProductoMapper
{
    public static PrecioProductoDto ToDto(this PrecioProducto entidad, bool esVigente) =>
        new(
            entidad.Id,
            entidad.ProductoId,
            entidad.Precio,
            entidad.VigenteDesdeUtc,
            entidad.FechaCreacionUtc,
            esVigente);
}
