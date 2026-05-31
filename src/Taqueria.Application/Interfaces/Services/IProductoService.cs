namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Productos;

public interface IProductoService
{
    Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProductoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken cancellationToken = default);
    Task<ProductoDto> ActualizarAsync(int id, ActualizarProductoDto dto, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
