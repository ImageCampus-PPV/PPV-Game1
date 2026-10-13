using GreenAbyss.Entities;
using GreenAbyss.Events;
using GreenAbyss.Services;

namespace GreenAbyss.Audio
{
    public class AudioManager
    {
        private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public void Init()
        {
            if (EventBus == null)
                return;

            EventBus.Subscribe<SetWwiseState>(OnSetState);
            EventBus.Subscribe<PostWwiseEvent>(OnPostEvent);
            EventBus.Subscribe<RegisterWwiseGameObject>(OnRegisterGameObject);
            EventBus.Subscribe<SetWwiseSwitch>(OnSetSwitch);
            EventBus.Subscribe<SetWwiseRTPCValue>(OnSetRTPC);
        }

        public void DeInit()
        {
            if (EventBus == null)
                return;

            EventBus.Unsubscribe<SetWwiseState>(OnSetState);
            EventBus.Unsubscribe<PostWwiseEvent>(OnPostEvent);
            EventBus.Unsubscribe<RegisterWwiseGameObject>(OnRegisterGameObject);
            EventBus.Unsubscribe<SetWwiseSwitch>(OnSetSwitch);
            EventBus.Unsubscribe<SetWwiseRTPCValue>(OnSetRTPC);
        }

        private void OnSetRTPC(in SetWwiseRTPCValue callback)
        {
            AkUnitySoundEngine.SetRTPCValue(callback.RtpcName, callback.RtpcValue);
        }

        private void OnSetSwitch(in SetWwiseSwitch callback)
        {
            AkUnitySoundEngine.SetSwitch(callback.SwitchGroupName, callback.SwitchStateName, callback.SoundEmitterGameObjectID);
        }

        private void OnRegisterGameObject(in RegisterWwiseGameObject callback)
        {
            AkUnitySoundEngine.RegisterGameObj(EntityRegistry.GetAs<Entities.BaseEntity>(callback.entityID).gameObject);
        }

        private void OnPostEvent(in PostWwiseEvent callback)
        {
            AkUnitySoundEngine.PostEvent(callback.EventName, callback.SoundEmitterGameObjectID);
        }

        private void OnSetState(in SetWwiseState callback)
        {
            AkUnitySoundEngine.SetState(callback.StateGroupName, callback.StateName);
        }
    }

}
