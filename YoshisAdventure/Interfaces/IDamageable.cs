using YoshisAdventure.GameObjects;

public interface IDamageable
{
    public int Health { get; }

    public int MaxHealth { get; }

    public void TakeDamage(int damage, GameObject source);

    public void Die(bool ClearHealth);
}