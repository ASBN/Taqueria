namespace Taqueria.Application.DTOs.Comandas;

public sealed record CrearComandaDto(int MesaId);

public sealed record AgregarComensalDto(int Numero);

public sealed record AgregarPartidaDto(int ProductoId, int Cantidad);

public sealed record CancelarComandaDto(string Motivo);

public sealed record EntregaParcialMeseroDto(
    int Id,
    int Cantidad,
    DateTime FechaHoraListaUtc,
    DateTime? FechaHoraEntregaUtc,
    string Estado);

public sealed record MermaComandaDetalleDto(
    int Id,
    int Cantidad,
    decimal ImporteHistorico,
    string Motivo,
    DateTime FechaHoraRegistroUtc,
    string RegistradaPor);

public sealed record ComandaDetalleDto(
    int Id,
    int ProductoId,
    int CategoriaProductoIdHistorico,
    string NombreCategoriaHistorico,
    string NombreProductoHistorico,
    decimal PrecioUnitarioHistorico,
    int Cantidad,
    int CantidadPreparada,
    int CantidadEntregada,
    decimal Subtotal,
    IReadOnlyList<EntregaParcialMeseroDto> EntregasParciales,
    IReadOnlyList<MermaComandaDetalleDto> Mermas);

public sealed record ComensalDto(
    int Id,
    int Numero,
    bool Activo,
    decimal Subtotal,
    IReadOnlyList<ComandaDetalleDto> Detalles);

public sealed record ComandaDto(
    int Id,
    string Folio,
    int MesaId,
    int MesaNumero,
    int UsuarioMeseroId,
    string Mesero,
    string Estado,
    DateTime FechaHoraAperturaUtc,
    DateTime? FechaHoraEnvioCocinaUtc,
    DateTime? FechaHoraEntregaUtc,
    DateTime? FechaHoraCancelacionUtc,
    string? MotivoCancelacion,
    string? CanceladaPor,
    decimal Total,
    IReadOnlyList<ComensalDto> Comensales);
