using YoshisAdventure.GameObjects;

namespace YoshisAdventure.Interfaces
{
    public interface IProjectile
    {
        int Damage { get; }

        GameObject Owner { get; }
    }
}