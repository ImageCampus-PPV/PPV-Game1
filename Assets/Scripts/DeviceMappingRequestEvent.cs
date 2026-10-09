using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

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