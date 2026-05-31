using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class ComandaRepository : IComandaRepository
{
    private readonly TaqueriaDbContext _context;

    public ComandaRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public Task<Comanda?> ObtenerAbiertaPorMesaAsync(int mesaId, CancellationToken cancellationToken = default) =>
        _context.Comandas.FirstOrDefaultAsync(
            x => x.MesaId == mesaId && x.Estado != EstadoComanda.Cobrada && x.Estado != EstadoComanda.Cancelada,
            cancellationToken);

    public async Task<IReadOnlyList<Comanda>> ObtenerActivasAsync(CancellationToken cancellationToken = default) =>
        await _context.Comandas.AsNoTracking()
            .Where(x => x.Estado != EstadoComanda.Cobrada && x.Estado != EstadoComanda.Cancelada)
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioMesero)
            .Include(x => x.UsuarioCancelacion)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.EntregasParciales)
            .OrderByDescending(x => x.FechaHoraAperturaUtc)
            .ToListAsync(cancellationToken);

    public Task<Comanda?> ObtenerCompletaPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Comandas
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioMesero)
            .Include(x => x.UsuarioCancelacion)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.EntregasParciales).ThenInclude(x => x.UsuarioCocina)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.EntregasParciales).ThenInclude(x => x.UsuarioMeseroEntrega)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.Mermas).ThenInclude(x => x.UsuarioRegistro)
            .Include(x => x.Pagos).ThenInclude(x => x.UsuarioCobro)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Comanda>> ObtenerActivasParaCocinaAsync(
        bool incluirListas,
        CancellationToken cancellationToken = default) =>
        await _context.Comandas.AsNoTracking()
            .Where(x => x.Estado == EstadoComanda.EnPreparacion
                || x.Estado == EstadoComanda.ParcialmenteLista
                || (incluirListas && x.Estado == EstadoComanda.Lista))
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioMesero)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.EntregasParciales).ThenInclude(x => x.UsuarioCocina)
            .OrderBy(x => x.Estado == EstadoComanda.Lista)
            .ThenBy(x => x.FechaHoraEnvioCocinaUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Comanda>> ObtenerPendientesCobroAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Comandas.AsNoTracking()
            .Where(x => x.Estado == EstadoComanda.PendienteCobro)
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioMesero)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles)
            .OrderBy(x => x.FechaHoraEntregaUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Comanda>> ObtenerCobradasPorFechaCobroAsync(
        DateTime desdeUtc,
        DateTime hastaUtcExclusiva,
        CancellationToken cancellationToken = default) =>
        await _context.Comandas.AsNoTracking()
            .Where(x => x.Estado == EstadoComanda.Cobrada
                && x.FechaHoraCobroUtc >= desdeUtc
                && x.FechaHoraCobroUtc < hastaUtcExclusiva)
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioMesero)
            .Include(x => x.Pagos)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles)
            .OrderBy(x => x.FechaHoraCobroUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Comanda>> ObtenerCanceladasConMermasPorFechaAsync(
        DateTime desdeUtc,
        DateTime hastaUtcExclusiva,
        CancellationToken cancellationToken = default) =>
        await _context.Comandas.AsNoTracking()
            .Where(x => x.Estado == EstadoComanda.Cancelada
                && x.FechaHoraCancelacionUtc >= desdeUtc
                && x.FechaHoraCancelacionUtc < hastaUtcExclusiva)
            .Include(x => x.Mesa)
            .Include(x => x.UsuarioCancelacion)
            .Include(x => x.Comensales).ThenInclude(x => x.Detalles).ThenInclude(x => x.Mermas).ThenInclude(x => x.UsuarioRegistro)
            .OrderBy(x => x.FechaHoraCancelacionUtc)
            .ToListAsync(cancellationToken);

    public Task AgregarAsync(Comanda comanda, CancellationToken cancellationToken = default) =>
        _context.Comandas.AddAsync(comanda, cancellationToken).AsTask();
}
