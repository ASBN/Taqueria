namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Productos;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class ProductoService : IProductoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var productos = await _unitOfWork.Productos.ObtenerTodosAsync(cancellationToken);
        var ahora = DateTime.UtcNow;
        return productos.Select(x => x.ToDto(ahora)).ToList();
    }

    public async Task<ProductoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await ObtenerEntidadAsync(id, cancellationToken);
        return producto.ToDto(DateTime.UtcNow);
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarCategoriaAsync(dto.CategoriaProductoId, cancellationToken);

        if (await _unitOfWork.Productos.ExisteNombreAsync(dto.Nombre, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe el producto {dto.Nombre}.");
        }

        var producto = new Producto(dto.CategoriaProductoId, dto.Nombre, dto.OrdenVisual);
        await _unitOfWork.Productos.AgregarAsync(producto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await ObtenerEntidadAsync(producto.Id, cancellationToken)).ToDto(DateTime.UtcNow);
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ActualizarProductoDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarCategoriaAsync(dto.CategoriaProductoId, cancellationToken);
        var producto = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.Productos.ExisteNombreAsync(dto.Nombre, id, cancellationToken))
        {
            throw new ConflictException($"Ya existe el producto {dto.Nombre}.");
        }

        producto.Actualizar(dto.Nombre, dto.OrdenVisual, dto.Activo, dto.CategoriaProductoId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(id, cancellationToken)).ToDto(DateTime.UtcNow);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.Productos.FueVendidoAsync(id, cancellationToken) || producto.Precios.Any())
        {
            producto.Desactivar();
        }
        else
        {
            _unitOfWork.Productos.Eliminar(producto);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Producto> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Productos.ObtenerConPreciosAsync(id, cancellationToken)
            ?? throw new NotFoundException("El producto solicitado no existe.");

    private async Task ValidarCategoriaAsync(int categoriaId, CancellationToken cancellationToken)
    {
        var categoria = await _unitOfWork.CategoriasProducto.ObtenerPorIdAsync(categoriaId, cancellationToken);
        if (categoria is null || !categoria.Activa)
        {
            throw new BusinessValidationException("Seleccione una categoría activa.");
        }
    }
}
