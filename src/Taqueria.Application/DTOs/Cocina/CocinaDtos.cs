namespace Taqueria.Application.DTOs.Cocina;

public sealed record RegistrarEntregaParcialDto(int Cantidad);

public sealed record EntregaParcialCocinaDto(
    int Id,
    int Cantidad,
    DateTime FechaHoraListaUtc,
    string Estado,
    string UsuarioCocina);

public sealed record PartidaCocinaDto(
    int Id,
    int ComensalId,
    int NumeroComensal,
    string Producto,
    int Cantidad,
    int CantidadPreparada,
    int CantidadPendiente,
    IReadOnlyList<EntregaParcialCocinaDto> EntregasParciales);

public sealed record ComensalCocinaDto(
    int Id,
    int Numero,
    IReadOnlyList<PartidaCocinaDto> Partidas);

public sealed record ComandaCocinaDto(
    int Id,
    string Folio,
    int MesaId,
    int MesaNumero,
    string Mesero,
    string Estado,
    DateTime? FechaHoraEnvioCocinaUtc,
    int TotalProductos,
    int TotalPreparados,
    int TotalPendientes,
    IReadOnlyList<ComensalCocinaDto> Comensales);
