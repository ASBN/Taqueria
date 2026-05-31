namespace Taqueria.Application.DTOs.Productos;

public sealed record CrearProductoDto(int CategoriaProductoId, string Nombre, int OrdenVisual);

public sealed record ActualizarProductoDto(int CategoriaProductoId, string Nombre, int OrdenVisual, bool Activo);

public sealed record ProductoDto(
    int Id,
    int CategoriaProductoId,
    string CategoriaNombre,
    string Nombre,
    int OrdenVisual,
    bool Activo,
    decimal? PrecioVigente);
