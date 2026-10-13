using GreenAbyss.Events;

public struct DeviceMappingRequestEvent : IEvent
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