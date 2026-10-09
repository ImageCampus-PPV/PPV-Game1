using ImageCampus.ToolBox.Events;

public class EnemySpawner : BaseEntity
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