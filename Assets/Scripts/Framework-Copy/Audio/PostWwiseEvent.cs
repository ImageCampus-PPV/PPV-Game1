using GreenAbyss.Events;

namespace GreenAbyss.Audio
{
    public struct PostWwiseEvent : IEvent
    {
        public string EventName;
        public ulong SoundEmitterGameObjectID;

        public void Assign(params object[] parameters)
        {
            EventName = (string)parameters[0];
            SoundEmitterGameObjectID = (ulong)parameters[1];
        }

        public void Reset()
        {
            EventName = "";
            SoundEmitterGameObjectID = 0;
        }
    }

}
