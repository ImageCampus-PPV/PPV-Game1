using GreenAbyss.Entities;

public class Wall : GreenAbyss.Entities.BaseEntity
{
    private void Awake()
    {
        gameObject.AddComponent<ForceEntityRegistry>();
    }
}