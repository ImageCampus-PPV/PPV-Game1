using ImageCampus.ToolBox.Events;

struct OnCombatDamage : IEvent
{
    public uint EntityToDamageID;
    public float DamageToReceive;

    public void Assign(params object[] parameters)
    {
        EntityToDamageID = (uint)parameters[0];
        DamageToReceive = (float)parameters[1];
    }

    public void Reset()
    {
        EntityToDamageID = default(uint);
        DamageToReceive = default(float);
    }
}