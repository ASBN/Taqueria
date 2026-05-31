namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Mesas;

public interface IMesaService
{
    Task<IReadOnlyList<MesaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<MesaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MesaDto> CrearAsync(CrearMesaDto dto, CancellationToken cancellationToken = default);
    Task<MesaDto> ActualizarAsync(int id, ActualizarMesaDto dto, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
