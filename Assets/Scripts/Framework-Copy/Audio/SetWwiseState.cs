using GreenAbyss.Events;

namespace GreenAbyss.Audio
{
public struct SetWwiseState : IEvent
{
    public string StateGroupName;
    public string StateName;

    public void Assign(params object[] parameters)
    {
        StateGroupName = (string)parameters[0];
        StateName = (string)parameters[1];
    }

    public void Reset()
    {
        StateGroupName = "";
        StateName = "";
    }
}

}
