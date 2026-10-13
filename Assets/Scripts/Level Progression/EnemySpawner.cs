using GreenAbyss.Events;

public class EnemySpawner : GreenAbyss.Entities.BaseEntity
{

}

public struct OnHordeStartedEvent : IEvent
{
    public void Assign(params object[] parameters)
    {
    }

    public void Reset()
    {
    }
}