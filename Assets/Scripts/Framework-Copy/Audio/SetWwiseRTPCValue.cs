using GreenAbyss.Events;

namespace GreenAbyss.Audio
{
    public struct SetWwiseRTPCValue : IEvent
    {
        public string RtpcName;
        public float RtpcValue;

        public void Assign(params object[] parameters)
        {
            RtpcName = (string)parameters[0];
            RtpcValue = (float)parameters[1];
        }

        public void Reset()
        {
            RtpcName = "";
            RtpcValue = 0f;
        }
    }
}
