using ImageCampus.ToolBox.Events;

public struct OnCombatDamage : IEvent
{
    public uint entityToDamageID;
    public float damageToReceive;

    public void Assign(params object[] parameters)
    {
        entityToDamageID = (uint)parameters[0];
        damageToReceive = (float)parameters[1];
    }

    public void Reset()
    {
        entityToDamageID = default(uint);
        damageToReceive = default(float);
    }
}