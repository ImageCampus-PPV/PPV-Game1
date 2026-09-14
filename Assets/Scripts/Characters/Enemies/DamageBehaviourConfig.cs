using UnityEngine;

public abstract class DamageBehaviourConfig : ScriptableObject
{
    public abstract DamageBehaviour CreateBehaviour();

}
