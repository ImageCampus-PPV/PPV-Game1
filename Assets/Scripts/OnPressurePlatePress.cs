using ImageCampus.ToolBox.Events;

public struct OnPressurePlatePress : IEvent
{
    public uint pressurePlateID;

    public void Assign(params object[] parameters)
    {
        pressurePlateID = (uint)parameters[0];
    }

    public void Reset()
    {
        pressurePlateID = default(uint);
    }
}
