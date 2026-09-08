using ImageCampus.ToolBox.Events;

public struct PlayerRequestInteractionAccepted<ObjectType> : IEvent where ObjectType : Interactable
{
    public uint characterID;
    public uint interactableID;

    public void Assign(params object[] parameters)
    {
        characterID = (uint)parameters[0];
        interactableID = (uint)parameters[1];
    }

    public void Reset()
    {
        characterID = default(uint);
        interactableID = default(uint);
    }
}
