namespace Taqueria.Application.DTOs.Mesas;

public sealed record CrearMesaDto(int Numero, int Capacidad);

public sealed record ActualizarMesaDto(int Numero, int Capacidad, bool Activa);

public sealed record MesaDto(int Id, int Numero, int Capacidad, bool Activa);
