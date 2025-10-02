using YoshisAdventure.GameObjects;

public interface IDamageable
{
    int Health { get; }

    void TakeDamage(int damage, GameObject source);

    void Die();
}