namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class Comensal
{
    private Comensal()
    {
    }

    public Comensal(int comandaId, int numero)
    {
        ComandaId = comandaId > 0 ? comandaId : throw new DomainException("La comanda es requerida.");
        Numero = numero > 0 ? numero : throw new DomainException("El número de comensal debe ser mayor a cero.");
        Activo = true;
    }

    public int Id { get; private set; }
    public int ComandaId { get; private set; }
    public int Numero { get; private set; }
    public bool Activo { get; private set; }

    public Comanda? Comanda { get; private set; }
    public ICollection<ComandaDetalle> Detalles { get; private set; } = new List<ComandaDetalle>();

    public ComandaDetalle AgregarPartida(Producto producto, PrecioProducto precio, int cantidad)
    {
        if (!Activo)
        {
            throw new DomainException("No se pueden agregar partidas a un comensal inactivo.");
        }

        if (Id <= 0)
        {
            throw new DomainException("El comensal debe existir antes de agregar partidas.");
        }

        if (producto.Id != precio.ProductoId)
        {
            throw new DomainException("El precio seleccionado no corresponde al producto.");
        }

        var categoria = producto.CategoriaProducto
            ?? throw new DomainException("El producto debe incluir su categoría para capturar una partida.");

        var partida = new ComandaDetalle(
            Id,
            producto.Id,
            categoria.Id,
            categoria.Nombre,
            producto.Nombre,
            precio.Precio,
            cantidad);

        Detalles.Add(partida);
        return partida;
    }

    public void QuitarPartida(int detalleId)
    {
        var partida = Detalles.FirstOrDefault(x => x.Id == detalleId)
            ?? throw new DomainException("La partida solicitada no pertenece al comensal.");

        if (partida.CantidadPreparada > 0 || partida.CantidadEntregada > 0)
        {
            throw new DomainException("Una partida ya preparada o entregada no puede retirarse.");
        }

        Detalles.Remove(partida);
    }
}
