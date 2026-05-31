namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;
using Taqueria.Domain.Enums;

public sealed class ComandaDetalle
{
    private ComandaDetalle()
    {
    }

    public ComandaDetalle(
        int comensalId,
        int productoId,
        int categoriaProductoIdHistorico,
        string nombreCategoriaHistorico,
        string nombreProductoHistorico,
        decimal precioUnitarioHistorico,
        int cantidad)
    {
        ComensalId = comensalId > 0 ? comensalId : throw new DomainException("El comensal es requerido.");
        ProductoId = productoId > 0 ? productoId : throw new DomainException("El producto es requerido.");
        CategoriaProductoIdHistorico = categoriaProductoIdHistorico > 0
            ? categoriaProductoIdHistorico
            : throw new DomainException("La categoría histórica es requerida.");
        NombreCategoriaHistorico = !string.IsNullOrWhiteSpace(nombreCategoriaHistorico)
            ? nombreCategoriaHistorico.Trim()
            : throw new DomainException("El nombre histórico de la categoría es requerido.");
        NombreProductoHistorico = !string.IsNullOrWhiteSpace(nombreProductoHistorico)
            ? nombreProductoHistorico.Trim()
            : throw new DomainException("El nombre histórico del producto es requerido.");
        PrecioUnitarioHistorico = precioUnitarioHistorico > 0
            ? decimal.Round(precioUnitarioHistorico, 2, MidpointRounding.AwayFromZero)
            : throw new DomainException("El precio histórico debe ser mayor a cero.");
        Cantidad = cantidad > 0 ? cantidad : throw new DomainException("La cantidad debe ser mayor a cero.");
        Subtotal = PrecioUnitarioHistorico * Cantidad;
        FechaHoraCapturaUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int ComensalId { get; private set; }
    public int ProductoId { get; private set; }
    public int CategoriaProductoIdHistorico { get; private set; }
    public string NombreCategoriaHistorico { get; private set; } = string.Empty;
    public string NombreProductoHistorico { get; private set; } = string.Empty;
    public decimal PrecioUnitarioHistorico { get; private set; }
    public int Cantidad { get; private set; }
    public int CantidadPreparada { get; private set; }
    public int CantidadEntregada { get; private set; }
    public decimal Subtotal { get; private set; }
    public DateTime FechaHoraCapturaUtc { get; private set; }

    public Comensal? Comensal { get; private set; }
    public Producto? Producto { get; private set; }
    public ICollection<EntregaParcial> EntregasParciales { get; private set; } = new List<EntregaParcial>();
    public ICollection<MermaComandaDetalle> Mermas { get; private set; } = new List<MermaComandaDetalle>();

    public EntregaParcial LiberarEntregaParcial(int cantidad, int usuarioCocinaId)
    {
        var entrega = new EntregaParcial(Id, cantidad, usuarioCocinaId);
        RegistrarPreparacion(cantidad);
        EntregasParciales.Add(entrega);
        return entrega;
    }

    public void RegistrarPreparacion(int cantidad)
    {
        if (cantidad <= 0 || CantidadPreparada + cantidad > Cantidad)
        {
            throw new DomainException("La cantidad preparada es inválida. No puede superar la cantidad pendiente.");
        }

        CantidadPreparada += cantidad;
    }

    public MermaComandaDetalle? RegistrarMermaPorCancelacion(string motivo, int usuarioRegistroId)
    {
        if (CantidadEntregada > 0)
        {
            throw new DomainException("Una partida ya entregada requiere un flujo de devolución, no una merma.");
        }

        if (CantidadPreparada == 0)
        {
            return null;
        }

        var cantidadMerma = CantidadPreparada;
        var merma = new MermaComandaDetalle(
            Id,
            cantidadMerma,
            PrecioUnitarioHistorico * cantidadMerma,
            motivo,
            usuarioRegistroId);

        foreach (var entrega in EntregasParciales.Where(x => x.Estado == EstadoEntregaParcial.ListaParaEntregar))
        {
            entrega.CancelarPorMerma();
        }

        Mermas.Add(merma);
        return merma;
    }

    public void RegistrarEntrega(int cantidad)
    {
        if (cantidad <= 0 || CantidadEntregada + cantidad > CantidadPreparada)
        {
            throw new DomainException("La cantidad entregada es inválida.");
        }

        CantidadEntregada += cantidad;
    }
}
