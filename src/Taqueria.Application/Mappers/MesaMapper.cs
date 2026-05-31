namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Domain.Entities;

public static class MesaMapper
{
    public static MesaDto ToDto(this Mesa entidad) =>
        new(entidad.Id, entidad.Numero, entidad.Capacidad, entidad.Activa);
}
