using ImageCampus.ToolBox.Events;

public struct OnCharacterDowned : IEvent
{
    public uint entityDownedID;
    public void Assign(params object[] parameters)
    {
        entityDownedID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityDownedID = default(uint);
    }
}
