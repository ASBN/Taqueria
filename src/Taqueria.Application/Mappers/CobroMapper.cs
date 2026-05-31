namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Cobros;
using Taqueria.Domain.Entities;

public static class CobroMapper
{
    public static CuentaPendienteCobroDto ToCuentaPendienteCobroDto(this Comanda entidad)
    {
        var partidas = entidad.Comensales.SelectMany(x => x.Detalles).ToList();
        return new CuentaPendienteCobroDto(
            entidad.Id,
            entidad.Folio,
            entidad.MesaId,
            entidad.Mesa?.Numero ?? 0,
            entidad.UsuarioMesero?.Nombre ?? string.Empty,
            entidad.Estado.ToString(),
            entidad.FechaHoraAperturaUtc,
            entidad.FechaHoraEntregaUtc,
            entidad.CalcularTotal(),
            partidas.Count,
            partidas.Sum(x => x.Cantidad));
    }

    public static CobroRegistradoDto ToCobroRegistradoDto(this Comanda entidad)
    {
        return new CobroRegistradoDto(
            entidad.Id,
            entidad.Mesa?.Numero ?? 0,
            entidad.Estado.ToString(),
            entidad.TotalCobrado ?? 0m,
            entidad.FechaHoraCobroUtc,
            entidad.Pagos
                .OrderBy(x => x.MetodoPago)
                .Select(pago => new PagoComandaDto(
                    pago.Id,
                    pago.MetodoPago.ToString(),
                    pago.Importe,
                    pago.FechaHoraPagoUtc,
                    pago.UsuarioCobro?.Nombre ?? string.Empty))
                .ToList());
    }
}
