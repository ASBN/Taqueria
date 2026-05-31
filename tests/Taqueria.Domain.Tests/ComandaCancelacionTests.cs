namespace Taqueria.Domain.Tests;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Exceptions;

public sealed class ComandaCancelacionTests
{
    [Fact]
    public void Cancelar_ComandaAbierta_RegistraMotivoUsuarioYEstado()
    {
        var comanda = new Comanda("TQ-PRUEBA", mesaId: 1, usuarioMeseroId: 2);

        comanda.Cancelar("Cliente se retiró.", usuarioCancelacionId: 2, esAdministrador: false);

        Assert.Equal(EstadoComanda.Cancelada, comanda.Estado);
        Assert.Equal("Cliente se retiró.", comanda.MotivoCancelacion);
        Assert.Equal(2, comanda.UsuarioCancelacionId);
        Assert.NotNull(comanda.FechaHoraCancelacionUtc);
    }

    [Fact]
    public void Cancelar_SinMotivo_RechazaLaOperacion()
    {
        var comanda = new Comanda("TQ-PRUEBA", mesaId: 1, usuarioMeseroId: 2);

        Assert.Throws<DomainException>(() => comanda.Cancelar(" ", usuarioCancelacionId: 2, esAdministrador: false));
    }
}
