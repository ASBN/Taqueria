namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class MermaComandaDetalle
{
    private MermaComandaDetalle()
    {
    }

    public MermaComandaDetalle(int comandaDetalleId, int cantidad, decimal importeHistorico, string motivo, int usuarioRegistroId)
    {
        ComandaDetalleId = comandaDetalleId > 0
            ? comandaDetalleId
            : throw new DomainException("La partida de la merma es requerida.");
        Cantidad = cantidad > 0
            ? cantidad
            : throw new DomainException("La cantidad de merma debe ser mayor a cero.");
        ImporteHistorico = importeHistorico >= 0
            ? decimal.Round(importeHistorico, 2, MidpointRounding.AwayFromZero)
            : throw new DomainException("El importe histórico de la merma no es válido.");
        Motivo = !string.IsNullOrWhiteSpace(motivo)
            ? motivo.Trim()
            : throw new DomainException("El motivo de la merma es requerido.");
        UsuarioRegistroId = usuarioRegistroId > 0
            ? usuarioRegistroId
            : throw new DomainException("El usuario que registra la merma es requerido.");
        FechaHoraRegistroUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int ComandaDetalleId { get; private set; }
    public int Cantidad { get; private set; }
    public decimal ImporteHistorico { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public DateTime FechaHoraRegistroUtc { get; private set; }
    public int UsuarioRegistroId { get; private set; }

    public ComandaDetalle? ComandaDetalle { get; private set; }
    public Usuario? UsuarioRegistro { get; private set; }
}
