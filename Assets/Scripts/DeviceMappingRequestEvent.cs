using ImageCampus.ToolBox.Events;

public struct DeviceMappingRequestEvent : IEvent
{
    public uint playerId;
    public void Assign(params object[] parameters)
    {
        playerId = (uint)parameters[0];
    }

    public void Reset()
    {
        playerId = default(uint);
    }
}