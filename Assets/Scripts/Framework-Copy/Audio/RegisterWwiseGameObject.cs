using GreenAbyss.Events;

namespace GreenAbyss.Audio
{
    public struct RegisterWwiseGameObject : IEvent
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

}
