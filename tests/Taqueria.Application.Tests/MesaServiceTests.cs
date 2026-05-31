namespace Taqueria.Application.Tests;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Services;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class MesaServiceTests
{
    [Fact]
    public async Task CrearAsync_ImpideNumeroDuplicado()
    {
        var unitOfWork = new FakeUnitOfWork(new[] { new Mesa(4, 4) });
        var service = new MesaService(unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() => service.CrearAsync(new CrearMesaDto(4, 2)));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IEnumerable<Mesa> mesas)
        {
            Mesas = new FakeMesaRepository(mesas);
        }

        public IUsuarioRepository Usuarios => throw new NotSupportedException();
        public IMesaRepository Mesas { get; }
        public IComandaRepository Comandas => throw new NotSupportedException();
        public IProductoRepository Productos => throw new NotSupportedException();
        public ICategoriaProductoRepository CategoriasProducto => throw new NotSupportedException();
        public IPrecioProductoRepository PreciosProducto => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeMesaRepository : IMesaRepository
    {
        private readonly List<Mesa> _mesas;

        public FakeMesaRepository(IEnumerable<Mesa> mesas)
        {
            _mesas = mesas.ToList();
        }

        public Task<IReadOnlyList<Mesa>> ObtenerTodasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Mesa>>(_mesas);

        public Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_mesas.FirstOrDefault(x => x.Id == id));

        public Task<bool> ExisteNumeroAsync(int numero, int? excluirId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(_mesas.Any(x => x.Numero == numero));

        public Task<bool> TieneComandasAsync(int mesaId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AgregarAsync(Mesa mesa, CancellationToken cancellationToken = default)
        {
            _mesas.Add(mesa);
            return Task.CompletedTask;
        }

        public void Eliminar(Mesa mesa) => _mesas.Remove(mesa);
    }
}
