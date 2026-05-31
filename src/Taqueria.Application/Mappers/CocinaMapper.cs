namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Domain.Entities;

public static class CocinaMapper
{
    public static ComandaCocinaDto ToCocinaDto(this Comanda entidad)
    {
        var comensales = entidad.Comensales
            .OrderBy(x => x.Numero)
            .Select(comensal =>
            {
                var partidas = comensal.Detalles
                    .OrderBy(x => x.FechaHoraCapturaUtc)
                    .Select(detalle => new PartidaCocinaDto(
                        detalle.Id,
                        comensal.Id,
                        comensal.Numero,
                        detalle.NombreProductoHistorico,
                        detalle.Cantidad,
                        detalle.CantidadPreparada,
                        detalle.Cantidad - detalle.CantidadPreparada,
                        detalle.EntregasParciales
                            .OrderBy(x => x.FechaHoraListaUtc)
                            .Select(entrega => new EntregaParcialCocinaDto(
                                entrega.Id,
                                entrega.Cantidad,
                                entrega.FechaHoraListaUtc,
                                entrega.Estado.ToString(),
                                entrega.UsuarioCocina?.Nombre ?? string.Empty))
                            .ToList()))
                    .ToList();

                return new ComensalCocinaDto(comensal.Id, comensal.Numero, partidas);
            })
            .ToList();

        var todasPartidas = comensales.SelectMany(x => x.Partidas).ToList();

        return new ComandaCocinaDto(
            entidad.Id,
            entidad.Folio,
            entidad.MesaId,
            entidad.Mesa?.Numero ?? 0,
            entidad.UsuarioMesero?.Nombre ?? string.Empty,
            entidad.Estado.ToString(),
            entidad.FechaHoraEnvioCocinaUtc,
            todasPartidas.Sum(x => x.Cantidad),
            todasPartidas.Sum(x => x.CantidadPreparada),
            todasPartidas.Sum(x => x.CantidadPendiente),
            comensales);
    }
}
