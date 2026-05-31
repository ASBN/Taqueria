namespace Taqueria.Domain.Tests;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class ComandaDetalleTests
{
    [Fact]
    public void Constructor_CopiaPrecioHistoricoYCalculaSubtotal()
    {
        var detalle = new ComandaDetalle(1, 1, 1, "Tacos", "Taco Pastor", 22m, 3);

        Assert.Equal(66m, detalle.Subtotal);
        Assert.Equal("Tacos", detalle.NombreCategoriaHistorico);
        Assert.Equal("Taco Pastor", detalle.NombreProductoHistorico);
        Assert.Equal(22m, detalle.PrecioUnitarioHistorico);
    }

    [Fact]
    public void RegistrarPreparacion_PermiteLiberacionesParcialesHastaLaCantidadPedida()
    {
        var detalle = new ComandaDetalle(1, 1, 1, "Tacos", "Taco Pastor", 22m, 50);

        detalle.RegistrarPreparacion(20);
        detalle.RegistrarPreparacion(15);

        Assert.Equal(35, detalle.CantidadPreparada);
        Assert.Equal(15, detalle.Cantidad - detalle.CantidadPreparada);
        Assert.Throws<DomainException>(() => detalle.RegistrarPreparacion(16));
    }

    [Fact]
    public void RegistrarEntrega_NoPermiteEntregarMasDeLoPreparado()
    {
        var detalle = new ComandaDetalle(1, 1, 1, "Tacos", "Taco Pastor", 22m, 3);

        Assert.Throws<DomainException>(() => detalle.RegistrarEntrega(1));
    }
}
