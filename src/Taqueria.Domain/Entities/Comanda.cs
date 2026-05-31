namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Exceptions;

public sealed class Comanda
{
    private Comanda()
    {
    }

    public Comanda(string folio, int mesaId, int usuarioMeseroId)
    {
        Folio = !string.IsNullOrWhiteSpace(folio)
            ? folio.Trim()
            : throw new DomainException("El folio es requerido.");
        MesaId = mesaId > 0 ? mesaId : throw new DomainException("La mesa es requerida.");
        UsuarioMeseroId = usuarioMeseroId > 0 ? usuarioMeseroId : throw new DomainException("El mesero es requerido.");
        Estado = EstadoComanda.Abierta;
        FechaHoraAperturaUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Folio { get; private set; } = string.Empty;
    public int MesaId { get; private set; }
    public int UsuarioMeseroId { get; private set; }
    public EstadoComanda Estado { get; private set; }
    public DateTime FechaHoraAperturaUtc { get; private set; }
    public DateTime? FechaHoraEnvioCocinaUtc { get; private set; }
    public DateTime? FechaHoraEntregaUtc { get; private set; }
    public DateTime? FechaHoraCobroUtc { get; private set; }
    public DateTime? FechaHoraCancelacionUtc { get; private set; }
    public string? MotivoCancelacion { get; private set; }
    public decimal? TotalCobrado { get; private set; }
    public int? UsuarioCancelacionId { get; private set; }

    public Mesa? Mesa { get; private set; }
    public Usuario? UsuarioMesero { get; private set; }
    public Usuario? UsuarioCancelacion { get; private set; }
    public ICollection<Comensal> Comensales { get; private set; } = new List<Comensal>();
    public ICollection<PagoComanda> Pagos { get; private set; } = new List<PagoComanda>();

    public decimal CalcularTotal() => Comensales.SelectMany(x => x.Detalles).Sum(x => x.Subtotal);

    public Comensal AgregarComensal(int numero)
    {
        AsegurarCapturaAbierta();

        if (Id <= 0)
        {
            throw new DomainException("La comanda debe existir antes de agregar comensales.");
        }

        if (Comensales.Any(x => x.Numero == numero && x.Activo))
        {
            throw new DomainException($"El comensal {numero} ya existe en esta comanda.");
        }

        var comensal = new Comensal(Id, numero);
        Comensales.Add(comensal);
        return comensal;
    }

    public void EnviarACocina()
    {
        AsegurarCapturaAbierta();

        if (!Comensales.SelectMany(x => x.Detalles).Any())
        {
            throw new DomainException("Agregue al menos una partida antes de enviar a cocina.");
        }

        Estado = EstadoComanda.EnPreparacion;
        FechaHoraEnvioCocinaUtc = DateTime.UtcNow;
    }

    public EntregaParcial LiberarEntregaParcial(int detalleId, int cantidad, int usuarioCocinaId)
    {
        AsegurarEnPreparacion();

        var detalle = BuscarDetalle(detalleId);
        var entrega = detalle.LiberarEntregaParcial(cantidad, usuarioCocinaId);
        ActualizarEstadoPreparacion();
        return entrega;
    }

    public EntregaParcial ConfirmarEntregaParcial(int entregaParcialId, int usuarioMeseroEntregaId)
    {
        AsegurarAtencionDeMesa();

        var detalle = Comensales
            .SelectMany(x => x.Detalles)
            .FirstOrDefault(x => x.EntregasParciales.Any(entrega => entrega.Id == entregaParcialId))
            ?? throw new DomainException("La entrega solicitada no pertenece a la comanda.");

        var entregaParcial = detalle.EntregasParciales.Single(x => x.Id == entregaParcialId);
        entregaParcial.ConfirmarEntrega(usuarioMeseroEntregaId);
        detalle.RegistrarEntrega(entregaParcial.Cantidad);
        ActualizarEstadoEntrega();
        return entregaParcial;
    }

    public void EnviarACobro()
    {
        if (Estado != EstadoComanda.Entregada)
        {
            throw new DomainException("La cuenta sólo puede enviarse a caja cuando toda la orden fue entregada a la mesa.");
        }

        Estado = EstadoComanda.PendienteCobro;
    }

    public IReadOnlyList<MermaComandaDetalle> Cancelar(string motivo, int usuarioCancelacionId, bool esAdministrador)
    {
        if (Estado == EstadoComanda.Cobrada)
        {
            throw new DomainException("Una comanda cobrada requiere un proceso de devolución y no puede cancelarse desde operación.");
        }

        if (Estado == EstadoComanda.Cancelada)
        {
            throw new DomainException("La comanda ya fue cancelada.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DomainException("Indique el motivo de cancelación.");
        }

        if (usuarioCancelacionId <= 0)
        {
            throw new DomainException("El usuario que cancela es requerido.");
        }

        if (!esAdministrador && Estado != EstadoComanda.Abierta)
        {
            throw new DomainException("El mesero sólo puede cancelar una comanda antes de enviarla a cocina.");
        }

        var detalles = Comensales.SelectMany(x => x.Detalles).ToList();
        if (detalles.Any(x => x.CantidadEntregada > 0))
        {
            throw new DomainException("La comanda ya tiene productos entregados. Requiere un flujo de devolución, no una merma.");
        }

        var motivoLimpio = motivo.Trim();
        var mermas = detalles
            .Select(x => x.RegistrarMermaPorCancelacion(motivoLimpio, usuarioCancelacionId))
            .Where(x => x is not null)
            .Cast<MermaComandaDetalle>()
            .ToList();

        MotivoCancelacion = motivoLimpio;
        UsuarioCancelacionId = usuarioCancelacionId;
        FechaHoraCancelacionUtc = DateTime.UtcNow;
        Estado = EstadoComanda.Cancelada;
        return mermas;
    }

    public IReadOnlyList<PagoComanda> RegistrarCobro(
        IReadOnlyCollection<(MetodoPago MetodoPago, decimal Importe)> pagosSolicitados,
        int usuarioCobroId)
    {
        if (Estado != EstadoComanda.PendienteCobro)
        {
            throw new DomainException("Sólo pueden cobrarse cuentas enviadas previamente a caja.");
        }

        if (Pagos.Any())
        {
            throw new DomainException("La comanda ya tiene pagos registrados.");
        }

        if (pagosSolicitados is null || pagosSolicitados.Count == 0)
        {
            throw new DomainException("Registre al menos un método de pago.");
        }

        if (pagosSolicitados.GroupBy(x => x.MetodoPago).Any(x => x.Count() > 1))
        {
            throw new DomainException("Cada método de pago sólo puede registrarse una vez por cuenta.");
        }

        if (pagosSolicitados.Any(x => x.Importe <= 0))
        {
            throw new DomainException("Cada importe pagado debe ser mayor a cero.");
        }

        var total = CalcularTotal();
        if (total <= 0)
        {
            throw new DomainException("No puede cobrarse una cuenta sin importe.");
        }

        var totalPagado = decimal.Round(pagosSolicitados.Sum(x => x.Importe), 2, MidpointRounding.AwayFromZero);
        if (totalPagado != total)
        {
            throw new DomainException("La suma de pagos debe coincidir exactamente con el total de la cuenta.");
        }

        var pagos = pagosSolicitados
            .Select(x => new PagoComanda(Id, x.MetodoPago, x.Importe, usuarioCobroId))
            .ToList();

        foreach (var pago in pagos)
        {
            Pagos.Add(pago);
        }

        TotalCobrado = total;
        FechaHoraCobroUtc = DateTime.UtcNow;
        Estado = EstadoComanda.Cobrada;
        return pagos;
    }

    public void AsegurarCapturaAbierta()
    {
        if (Estado != EstadoComanda.Abierta)
        {
            throw new DomainException("La comanda ya fue enviada a cocina y no admite cambios de captura en esta fase.");
        }
    }

    private ComandaDetalle BuscarDetalle(int detalleId) =>
        Comensales
            .SelectMany(x => x.Detalles)
            .FirstOrDefault(x => x.Id == detalleId)
            ?? throw new DomainException("La partida solicitada no pertenece a la comanda.");

    private void AsegurarEnPreparacion()
    {
        if (Estado != EstadoComanda.EnPreparacion && Estado != EstadoComanda.ParcialmenteLista)
        {
            throw new DomainException("Sólo pueden liberarse partidas de comandas pendientes en cocina.");
        }
    }

    private void AsegurarAtencionDeMesa()
    {
        if (Estado != EstadoComanda.EnPreparacion
            && Estado != EstadoComanda.ParcialmenteLista
            && Estado != EstadoComanda.Lista)
        {
            throw new DomainException("No hay entregas parciales pendientes de confirmar en esta comanda.");
        }
    }

    private void ActualizarEstadoPreparacion()
    {
        var detalles = Comensales.SelectMany(x => x.Detalles).ToList();

        if (detalles.All(x => x.CantidadPreparada == x.Cantidad))
        {
            Estado = EstadoComanda.Lista;
            return;
        }

        Estado = detalles.Any(x => x.CantidadPreparada > 0)
            ? EstadoComanda.ParcialmenteLista
            : EstadoComanda.EnPreparacion;
    }

    private void ActualizarEstadoEntrega()
    {
        var detalles = Comensales.SelectMany(x => x.Detalles).ToList();
        if (detalles.Count > 0 && detalles.All(x => x.CantidadEntregada == x.Cantidad))
        {
            Estado = EstadoComanda.Entregada;
            FechaHoraEntregaUtc = DateTime.UtcNow;
        }
    }
}
