namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.CategoriasProducto;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class CategoriaProductoService : ICategoriaProductoService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaProductoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoriaProductoDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _unitOfWork.CategoriasProducto.ObtenerTodasAsync(cancellationToken);
        return categorias.Select(x => x.ToDto()).ToList();
    }

    public async Task<CategoriaProductoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObtenerEntidadAsync(id, cancellationToken);
        return categoria.ToDto();
    }

    public async Task<CategoriaProductoDto> CrearAsync(CrearCategoriaProductoDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.CategoriasProducto.ExisteNombreAsync(dto.Nombre, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe la categoría {dto.Nombre}.");
        }

        var categoria = new CategoriaProducto(dto.Nombre, dto.OrdenVisual);
        await _unitOfWork.CategoriasProducto.AgregarAsync(categoria, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return categoria.ToDto();
    }

    public async Task<CategoriaProductoDto> ActualizarAsync(int id, ActualizarCategoriaProductoDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.CategoriasProducto.ExisteNombreAsync(dto.Nombre, id, cancellationToken))
        {
            throw new ConflictException($"Ya existe la categoría {dto.Nombre}.");
        }

        categoria.Actualizar(dto.Nombre, dto.OrdenVisual, dto.Activa);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return categoria.ToDto();
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.CategoriasProducto.TieneProductosAsync(id, cancellationToken))
        {
            categoria.Desactivar();
        }
        else
        {
            _unitOfWork.CategoriasProducto.Eliminar(categoria);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CategoriaProducto> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.CategoriasProducto.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La categoría solicitada no existe.");
}
