namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.PreciosProducto;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class PrecioProductoService : IPrecioProductoService
{
    private readonly IUnitOfWork _unitOfWork;

    public PrecioProductoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PrecioProductoDto>> ObtenerPorProductoAsync(int productoId, CancellationToken cancellationToken = default)
    {
        await ValidarProductoAsync(productoId, cancellationToken);
        var precios = await _unitOfWork.PreciosProducto.ObtenerPorProductoAsync(productoId, cancellationToken);
        var vigente = await _unitOfWork.PreciosProducto.ObtenerVigenteAsync(productoId, DateTime.UtcNow, cancellationToken);

        return precios.Select(x => x.ToDto(vigente?.Id == x.Id)).ToList();
    }

    public async Task<PrecioProductoDto?> ObtenerVigenteAsync(int productoId, CancellationToken cancellationToken = default)
    {
        await ValidarProductoAsync(productoId, cancellationToken);
        var precio = await _unitOfWork.PreciosProducto.ObtenerVigenteAsync(productoId, DateTime.UtcNow, cancellationToken);
        return precio?.ToDto(true);
    }

    public async Task<PrecioProductoDto> CrearAsync(int productoId, CrearPrecioProductoDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarProductoAsync(productoId, cancellationToken);
        var precio = new PrecioProducto(productoId, dto.Precio, dto.VigenteDesdeUtc);
        await _unitOfWork.PreciosProducto.AgregarAsync(precio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var vigente = await _unitOfWork.PreciosProducto.ObtenerVigenteAsync(productoId, DateTime.UtcNow, cancellationToken);
        return precio.ToDto(vigente?.Id == precio.Id);
    }

    public async Task<PrecioProductoDto> ActualizarAsync(int id, ActualizarPrecioProductoDto dto, CancellationToken cancellationToken = default)
    {
        var precio = await ObtenerEditableAsync(id, cancellationToken);

        if (dto.VigenteDesdeUtc <= DateTime.UtcNow)
        {
            throw new BusinessValidationException("Una corrección de precio programado debe conservar una vigencia futura.");
        }

        precio.Actualizar(dto.Precio, dto.VigenteDesdeUtc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var vigente = await _unitOfWork.PreciosProducto.ObtenerVigenteAsync(precio.ProductoId, DateTime.UtcNow, cancellationToken);
        return precio.ToDto(vigente?.Id == precio.Id);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var precio = await ObtenerEditableAsync(id, cancellationToken);
        _unitOfWork.PreciosProducto.Eliminar(precio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<PrecioProducto> ObtenerEditableAsync(int id, CancellationToken cancellationToken)
    {
        var precio = await _unitOfWork.PreciosProducto.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("El precio solicitado no existe.");

        if (precio.VigenteDesdeUtc <= DateTime.UtcNow)
        {
            throw new BusinessValidationException(
                "Un precio que ya entró en vigencia conserva el histórico. Registre una nueva vigencia en lugar de editarlo.");
        }

        return precio;
    }

    private async Task ValidarProductoAsync(int productoId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Productos.ObtenerPorIdAsync(productoId, cancellationToken) is null)
        {
            throw new NotFoundException("El producto solicitado no existe.");
        }
    }
}
