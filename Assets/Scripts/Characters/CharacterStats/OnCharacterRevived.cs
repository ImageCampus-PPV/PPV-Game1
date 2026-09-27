using ImageCampus.ToolBox.Events;

public struct OnCharacterRevived : IEvent
{
    public uint entityRevivedID;
    public void Assign(params object[] parameters)
    {
        entityRevivedID = (uint)parameters[0];
    }

    public void Reset()
    {
        entityRevivedID = default(uint);
    }
}
