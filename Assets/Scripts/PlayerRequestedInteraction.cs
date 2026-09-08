using ImageCampus.ToolBox.Events;

public struct PlayerRequestedInteraction : IEvent
{
    public uint entityID;

    public void Assign(params object[] parameters)
    {
        entityID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityID = default(uint);
    }
}