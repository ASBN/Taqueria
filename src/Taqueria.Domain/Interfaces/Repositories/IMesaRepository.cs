namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;

public interface IMesaRepository
{
    Task<IReadOnlyList<Mesa>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNumeroAsync(int numero, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> TieneComandasAsync(int mesaId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Mesa mesa, CancellationToken cancellationToken = default);
    void Eliminar(Mesa mesa);
}
