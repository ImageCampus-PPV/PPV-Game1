using ImageCampus.ToolBox.Events;

public class Nucleus : DamageableEntity
{
    public void OnHordeComplete()
    {
        if (IsDowned)
            Revive();
        else
            Heal(MaxHealth);
    }
}

public struct OnHordeEndedEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}