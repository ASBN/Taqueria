namespace Taqueria.Domain.Enums;

public enum EstadoComanda
{
    Abierta = 1,
    EnPreparacion = 2,
    ParcialmenteLista = 3,
    Lista = 4,
    Entregada = 5,
    Cobrada = 6,
    Cancelada = 7,
    PendienteCobro = 8
}
