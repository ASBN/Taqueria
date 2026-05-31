namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.PreciosProducto;

public interface IPrecioProductoService
{
    Task<IReadOnlyList<PrecioProductoDto>> ObtenerPorProductoAsync(int productoId, CancellationToken cancellationToken = default);
    Task<PrecioProductoDto?> ObtenerVigenteAsync(int productoId, CancellationToken cancellationToken = default);
    Task<PrecioProductoDto> CrearAsync(int productoId, CrearPrecioProductoDto dto, CancellationToken cancellationToken = default);
    Task<PrecioProductoDto> ActualizarAsync(int id, ActualizarPrecioProductoDto dto, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
