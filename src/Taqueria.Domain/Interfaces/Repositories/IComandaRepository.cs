namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;

public interface IComandaRepository
{
    Task<Comanda?> ObtenerAbiertaPorMesaAsync(int mesaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comanda>> ObtenerActivasAsync(CancellationToken cancellationToken = default);
    Task<Comanda?> ObtenerCompletaPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comanda>> ObtenerActivasParaCocinaAsync(bool incluirListas, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comanda>> ObtenerPendientesCobroAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comanda>> ObtenerCobradasPorFechaCobroAsync(DateTime desdeUtc, DateTime hastaUtcExclusiva, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comanda>> ObtenerCanceladasConMermasPorFechaAsync(DateTime desdeUtc, DateTime hastaUtcExclusiva, CancellationToken cancellationToken = default);
    Task AgregarAsync(Comanda comanda, CancellationToken cancellationToken = default);
}
