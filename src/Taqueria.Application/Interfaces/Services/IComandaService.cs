namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Comandas;

public interface IComandaService
{
    Task<IReadOnlyList<ComandaDto>> ObtenerActivasAsync(CancellationToken cancellationToken = default);
    Task<ComandaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ComandaDto> CrearAsync(CrearComandaDto dto, int usuarioMeseroId, CancellationToken cancellationToken = default);
    Task<ComandaDto> AgregarComensalAsync(int comandaId, AgregarComensalDto dto, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> AgregarPartidaAsync(int comandaId, int comensalId, AgregarPartidaDto dto, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> QuitarPartidaAsync(int comandaId, int comensalId, int detalleId, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> EnviarACocinaAsync(int comandaId, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> ConfirmarEntregaParcialAsync(int comandaId, int entregaParcialId, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> EnviarACobroAsync(int comandaId, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
    Task<ComandaDto> CancelarAsync(int comandaId, CancelarComandaDto dto, int usuarioId, bool esAdministrador, CancellationToken cancellationToken = default);
}
