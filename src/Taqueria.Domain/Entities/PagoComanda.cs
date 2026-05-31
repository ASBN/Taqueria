namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Exceptions;

public sealed class PagoComanda
{
    private PagoComanda()
    {
    }

    public PagoComanda(int comandaId, MetodoPago metodoPago, decimal importe, int usuarioCobroId)
    {
        ComandaId = comandaId > 0 ? comandaId : throw new DomainException("La comanda es requerida.");
        MetodoPago = metodoPago;
        Importe = importe > 0 ? decimal.Round(importe, 2, MidpointRounding.AwayFromZero) : throw new DomainException("El importe debe ser mayor a cero.");
        UsuarioCobroId = usuarioCobroId > 0 ? usuarioCobroId : throw new DomainException("El usuario que cobra es requerido.");
        FechaHoraPagoUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int ComandaId { get; private set; }
    public MetodoPago MetodoPago { get; private set; }
    public decimal Importe { get; private set; }
    public DateTime FechaHoraPagoUtc { get; private set; }
    public int UsuarioCobroId { get; private set; }

    public Comanda? Comanda { get; private set; }
    public Usuario? UsuarioCobro { get; private set; }
}
