using ImageCampus.ToolBox.Events;

namespace Pawgineers.Gameplay.Inputs
{
    public struct InputDeviceDisconnectRequestEvent : IEvent
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