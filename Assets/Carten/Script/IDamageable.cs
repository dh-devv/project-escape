namespace Carten
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        bool IsDead { get; }
    }
}