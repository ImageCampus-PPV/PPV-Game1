using System;

public interface IDamageable
{
    void TakeDamage(float amount);
    Action<float> OnTakeDamage { get; set; }
}
