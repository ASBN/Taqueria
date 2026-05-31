namespace Taqueria.Application.DTOs.CategoriasProducto;

public sealed record CrearCategoriaProductoDto(string Nombre, int OrdenVisual);

public sealed record ActualizarCategoriaProductoDto(string Nombre, int OrdenVisual, bool Activa);

public sealed record CategoriaProductoDto(int Id, string Nombre, int OrdenVisual, bool Activa);
