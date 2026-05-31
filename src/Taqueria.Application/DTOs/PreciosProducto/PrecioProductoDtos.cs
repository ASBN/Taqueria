namespace Taqueria.Application.DTOs.PreciosProducto;

public sealed record CrearPrecioProductoDto(decimal Precio, DateTime VigenteDesdeUtc);

public sealed record ActualizarPrecioProductoDto(decimal Precio, DateTime VigenteDesdeUtc);

public sealed record PrecioProductoDto(
    int Id,
    int ProductoId,
    decimal Precio,
    DateTime VigenteDesdeUtc,
    DateTime FechaCreacionUtc,
    bool EsVigente);
