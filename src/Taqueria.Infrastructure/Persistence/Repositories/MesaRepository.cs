using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class MesaRepository : IMesaRepository
{
    private readonly TaqueriaDbContext _context;

    public MesaRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Mesa>> ObtenerTodasAsync(CancellationToken cancellationToken = default) =>
        await _context.Mesas.AsNoTracking().OrderBy(x => x.Numero).ToListAsync(cancellationToken);

    public Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Mesas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExisteNumeroAsync(int numero, int? excluirId = null, CancellationToken cancellationToken = default) =>
        _context.Mesas.AnyAsync(x => x.Numero == numero && (!excluirId.HasValue || x.Id != excluirId.Value), cancellationToken);

    public Task<bool> TieneComandasAsync(int mesaId, CancellationToken cancellationToken = default) =>
        _context.Comandas.AnyAsync(x => x.MesaId == mesaId, cancellationToken);

    public Task AgregarAsync(Mesa mesa, CancellationToken cancellationToken = default) =>
        _context.Mesas.AddAsync(mesa, cancellationToken).AsTask();

    public void Eliminar(Mesa mesa) => _context.Mesas.Remove(mesa);
}
