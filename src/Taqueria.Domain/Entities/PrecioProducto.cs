namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class PrecioProducto
{
    private PrecioProducto()
    {
    }

    public PrecioProducto(int productoId, decimal precio, DateTime vigenteDesdeUtc)
    {
        ProductoId = productoId > 0
            ? productoId
            : throw new DomainException("El producto es requerido.");
        Actualizar(precio, vigenteDesdeUtc);
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int ProductoId { get; private set; }
    public decimal Precio { get; private set; }
    public DateTime VigenteDesdeUtc { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }
    public Producto? Producto { get; private set; }

    public void Actualizar(decimal precio, DateTime vigenteDesdeUtc)
    {
        if (precio <= 0)
        {
            throw new DomainException("El precio debe ser mayor a cero.");
        }

        Precio = decimal.Round(precio, 2, MidpointRounding.AwayFromZero);
        VigenteDesdeUtc = vigenteDesdeUtc;
    }
}
