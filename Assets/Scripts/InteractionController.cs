using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;

public class InteractionController : IInitiable, IDisposable
{
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
    private NearestObjectDetector PossibleInteractions => ServiceProvider.Instance.GetService<NearestObjectDetector>();

    public void Init()
    {
        EventBus.Subscribe<PlayerRequestedInteraction>(OnPlayerRequestedInteraction);
    }

    public void LateInit()
    {

    }

    private (uint objectID, Type objectType) CharacterInteraction(uint characterID)
    {
        return PossibleInteractions.InteractablePerCharacter[characterID];
    }

    private void OnPlayerRequestedInteraction(in PlayerRequestedInteraction playerRequestedInteractionEvent)
    {
        (uint objectID, Type objectType) = CharacterInteraction(playerRequestedInteractionEvent.entityID);

        if (objectID == BaseEntity.NULL_BASE_ENTITY)
            return;

        EventBus.Raise<PlayerRequestInteractionAcceptedGeneric>(playerRequestedInteractionEvent.entityID, objectID, objectType.Name);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<PlayerRequestedInteraction>(OnPlayerRequestedInteraction);
    }
}
