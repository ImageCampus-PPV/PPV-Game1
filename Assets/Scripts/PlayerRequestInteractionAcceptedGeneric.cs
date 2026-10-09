using ImageCampus.ToolBox.Events;

public struct PlayerRequestInteractionAcceptedGeneric : IEvent
{
    public uint characterID;
    public uint interactableID;
    public string interactableType;

    public void Assign(params object[] parameters)
    {
        characterID = (uint)parameters[0];
        interactableID = (uint)parameters[1];
        interactableType = (string)parameters[2];
    }

    public void Reset()
    {
        characterID = default(uint);
        interactableID = default(uint);
        interactableType = default(string);
    }
}
