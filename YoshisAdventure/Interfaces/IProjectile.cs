using Project6.GameObjects;

namespace Project6.Interfaces
{
    public interface IProjectile
    {
        int Damage { get; }

        GameObject Owner { get; }
    }
}