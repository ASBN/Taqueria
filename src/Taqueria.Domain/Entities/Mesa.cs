namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Exceptions;

public sealed class Mesa
{
    private Mesa()
    {
    }

    public Mesa(int numero, int capacidad)
    {
        Actualizar(numero, capacidad, true);
    }

    public int Id { get; private set; }
    public int Numero { get; private set; }
    public int Capacidad { get; private set; }
    public bool Activa { get; private set; }

    public ICollection<Comanda> Comandas { get; private set; } = new List<Comanda>();

    public void Actualizar(int numero, int capacidad, bool activa)
    {
        if (numero <= 0)
        {
            throw new DomainException("El número de mesa debe ser mayor a cero.");
        }

        if (capacidad <= 0)
        {
            throw new DomainException("La capacidad debe ser mayor a cero.");
        }

        Numero = numero;
        Capacidad = capacidad;
        Activa = activa;
    }

    public void Desactivar() => Activa = false;
}
