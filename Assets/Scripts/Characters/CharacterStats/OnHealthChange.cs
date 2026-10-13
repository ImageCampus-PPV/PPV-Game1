using GreenAbyss.Events;

public struct OnHealthChange : IEvent
{
    public uint entityAffectedID;
    public float currentHealth;
    public float maxHealth;

    public void Assign(params object[] parameters)
    {
        entityAffectedID = (uint)parameters[0];
        currentHealth = (float)parameters[1];
        maxHealth = (float)parameters[2];
    }

    public void Reset()
    {
        entityAffectedID = default(uint);
        currentHealth = default(float);
        maxHealth = default(float);
    }
}
