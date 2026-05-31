namespace Taqueria.Application.DTOs.Cobros;

public sealed record RegistrarPagoDto(string MetodoPago, decimal Importe);

// MetodoPago se conserva para compatibilidad con clientes de pago único.
public sealed record RegistrarCobroDto(
    string? MetodoPago = null,
    IReadOnlyList<RegistrarPagoDto>? Pagos = null);

public sealed record PagoComandaDto(
    int Id,
    string MetodoPago,
    decimal Importe,
    DateTime FechaHoraPagoUtc,
    string UsuarioCobro);

public sealed record CuentaPendienteCobroDto(
    int ComandaId,
    string Folio,
    int MesaId,
    int MesaNumero,
    string Mesero,
    string Estado,
    DateTime FechaHoraAperturaUtc,
    DateTime? FechaHoraEntregaUtc,
    decimal Total,
    int TotalPartidas,
    int TotalProductos);

public sealed record CobroRegistradoDto(
    int ComandaId,
    int MesaNumero,
    string Estado,
    decimal TotalCobrado,
    DateTime? FechaHoraCobroUtc,
    IReadOnlyList<PagoComandaDto> Pagos);
