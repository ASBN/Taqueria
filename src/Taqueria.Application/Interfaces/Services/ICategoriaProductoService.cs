namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.CategoriasProducto;

public interface ICategoriaProductoService
{
    Task<IReadOnlyList<CategoriaProductoDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<CategoriaProductoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoriaProductoDto> CrearAsync(CrearCategoriaProductoDto dto, CancellationToken cancellationToken = default);
    Task<CategoriaProductoDto> ActualizarAsync(int id, ActualizarCategoriaProductoDto dto, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
