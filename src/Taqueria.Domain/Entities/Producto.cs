namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class Producto
{
    private Producto()
    {
    }

    public Producto(int categoriaProductoId, string nombre, int ordenVisual)
    {
        CategoriaProductoId = categoriaProductoId > 0
            ? categoriaProductoId
            : throw new DomainException("La categoría del producto es requerida.");
        Actualizar(nombre, ordenVisual, true);
    }

    public int Id { get; private set; }
    public int CategoriaProductoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public int OrdenVisual { get; private set; }
    public bool Activo { get; private set; }

    public CategoriaProducto? CategoriaProducto { get; private set; }
    public ICollection<PrecioProducto> Precios { get; private set; } = new List<PrecioProducto>();

    public void Actualizar(string nombre, int ordenVisual, bool activo, int? categoriaProductoId = null)
    {
        Nombre = !string.IsNullOrWhiteSpace(nombre)
            ? nombre.Trim()
            : throw new DomainException("El nombre del producto es requerido.");

        if (ordenVisual < 0)
        {
            throw new DomainException("El orden visual no puede ser negativo.");
        }

        if (categoriaProductoId.HasValue)
        {
            CategoriaProductoId = categoriaProductoId.Value > 0
                ? categoriaProductoId.Value
                : throw new DomainException("La categoría del producto es requerida.");
        }

        OrdenVisual = ordenVisual;
        Activo = activo;
    }

    public PrecioProducto ObtenerPrecioVigente(DateTime fechaUtc)
    {
        return Precios
            .Where(precio => precio.VigenteDesdeUtc <= fechaUtc)
            .OrderByDescending(precio => precio.VigenteDesdeUtc)
            .FirstOrDefault()
            ?? throw new DomainException($"El producto {Nombre} no tiene un precio vigente.");
    }

    public void Desactivar() => Activo = false;
}
