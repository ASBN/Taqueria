namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class CategoriaProducto
{
    private CategoriaProducto()
    {
    }

    public CategoriaProducto(string nombre, int ordenVisual)
    {
        Actualizar(nombre, ordenVisual, true);
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public int OrdenVisual { get; private set; }
    public bool Activa { get; private set; }
    public ICollection<Producto> Productos { get; private set; } = new List<Producto>();

    public void Actualizar(string nombre, int ordenVisual, bool activa)
    {
        Nombre = !string.IsNullOrWhiteSpace(nombre)
            ? nombre.Trim()
            : throw new DomainException("El nombre de la categoría es requerido.");

        if (ordenVisual < 0)
        {
            throw new DomainException("El orden visual no puede ser negativo.");
        }

        OrdenVisual = ordenVisual;
        Activa = activa;
    }

    public void Desactivar() => Activa = false;
}
