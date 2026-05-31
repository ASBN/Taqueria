namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Exceptions;

public sealed class EntregaParcial
{
    private EntregaParcial()
    {
    }

    public EntregaParcial(int comandaDetalleId, int cantidad, int usuarioCocinaId)
    {
        ComandaDetalleId = comandaDetalleId > 0 ? comandaDetalleId : throw new DomainException("La partida es requerida.");
        Cantidad = cantidad > 0 ? cantidad : throw new DomainException("La cantidad liberada debe ser mayor a cero.");
        UsuarioCocinaId = usuarioCocinaId > 0 ? usuarioCocinaId : throw new DomainException("El usuario de cocina es requerido.");
        FechaHoraListaUtc = DateTime.UtcNow;
        Estado = EstadoEntregaParcial.ListaParaEntregar;
    }

    public int Id { get; private set; }
    public int ComandaDetalleId { get; private set; }
    public int Cantidad { get; private set; }
    public DateTime FechaHoraListaUtc { get; private set; }
    public int UsuarioCocinaId { get; private set; }
    public EstadoEntregaParcial Estado { get; private set; }
    public DateTime? FechaHoraEntregaUtc { get; private set; }
    public int? UsuarioMeseroEntregaId { get; private set; }

    public ComandaDetalle? ComandaDetalle { get; private set; }
    public Usuario? UsuarioCocina { get; private set; }
    public Usuario? UsuarioMeseroEntrega { get; private set; }

    public void CancelarPorMerma()
    {
        if (Estado == EstadoEntregaParcial.Entregada)
        {
            throw new DomainException("Una entrega ya confirmada no puede convertirse en merma.");
        }

        if (Estado == EstadoEntregaParcial.ListaParaEntregar)
        {
            Estado = EstadoEntregaParcial.Cancelada;
        }
    }

    public void ConfirmarEntrega(int usuarioMeseroEntregaId)
    {
        if (Estado != EstadoEntregaParcial.ListaParaEntregar)
        {
            throw new DomainException("La entrega parcial ya no está pendiente de entrega a la mesa.");
        }

        UsuarioMeseroEntregaId = usuarioMeseroEntregaId > 0
            ? usuarioMeseroEntregaId
            : throw new DomainException("El mesero que entrega es requerido.");
        FechaHoraEntregaUtc = DateTime.UtcNow;
        Estado = EstadoEntregaParcial.Entregada;
    }
}
