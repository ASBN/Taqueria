namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.CategoriasProducto;
using Taqueria.Domain.Entities;

public static class CategoriaProductoMapper
{
    public static CategoriaProductoDto ToDto(this CategoriaProducto entidad) =>
        new(entidad.Id, entidad.Nombre, entidad.OrdenVisual, entidad.Activa);
}
