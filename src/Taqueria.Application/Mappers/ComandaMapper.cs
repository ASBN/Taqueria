namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Domain.Entities;

public static class ComandaMapper
{
    public static ComandaDto ToDto(this Comanda entidad)
    {
        var comensales = entidad.Comensales
            .OrderBy(x => x.Numero)
            .Select(comensal =>
            {
                var detalles = comensal.Detalles
                    .OrderBy(x => x.FechaHoraCapturaUtc)
                    .Select(detalle => new ComandaDetalleDto(
                        detalle.Id,
                        detalle.ProductoId,
                        detalle.CategoriaProductoIdHistorico,
                        detalle.NombreCategoriaHistorico,
                        detalle.NombreProductoHistorico,
                        detalle.PrecioUnitarioHistorico,
                        detalle.Cantidad,
                        detalle.CantidadPreparada,
                        detalle.CantidadEntregada,
                        detalle.Subtotal,
                        detalle.EntregasParciales
                            .OrderBy(x => x.FechaHoraListaUtc)
                            .Select(entrega => new EntregaParcialMeseroDto(
                                entrega.Id,
                                entrega.Cantidad,
                                entrega.FechaHoraListaUtc,
                                entrega.FechaHoraEntregaUtc,
                                entrega.Estado.ToString()))
                            .ToList(),
                        detalle.Mermas
                            .OrderBy(x => x.FechaHoraRegistroUtc)
                            .Select(merma => new MermaComandaDetalleDto(
                                merma.Id,
                                merma.Cantidad,
                                merma.ImporteHistorico,
                                merma.Motivo,
                                merma.FechaHoraRegistroUtc,
                                merma.UsuarioRegistro?.Nombre ?? string.Empty))
                            .ToList()))
                    .ToList();

                return new ComensalDto(
                    comensal.Id,
                    comensal.Numero,
                    comensal.Activo,
                    detalles.Sum(x => x.Subtotal),
                    detalles);
            })
            .ToList();

        return new ComandaDto(
            entidad.Id,
            entidad.Folio,
            entidad.MesaId,
            entidad.Mesa?.Numero ?? 0,
            entidad.UsuarioMeseroId,
            entidad.UsuarioMesero?.Nombre ?? string.Empty,
            entidad.Estado.ToString(),
            entidad.FechaHoraAperturaUtc,
            entidad.FechaHoraEnvioCocinaUtc,
            entidad.FechaHoraEntregaUtc,
            entidad.FechaHoraCancelacionUtc,
            entidad.MotivoCancelacion,
            entidad.UsuarioCancelacion?.Nombre,
            entidad.CalcularTotal(),
            comensales);
    }
}
