using GreenAbyss.Events;

namespace GreenAbyss.Audio
{
    public struct SetWwiseSwitch : IEvent
    {
        public string SwitchGroupName;
        public string SwitchStateName;
        public ulong SoundEmitterGameObjectID;
        public void Assign(params object[] parameters)
        {
            SwitchGroupName = (string)parameters[0];
            SwitchStateName = (string)parameters[1];
            SoundEmitterGameObjectID = (ulong)parameters[2];
        }

        public void Reset()
        {
            SwitchGroupName = "";
            SwitchStateName = "";
            SoundEmitterGameObjectID = 0;
        }
    }

}
