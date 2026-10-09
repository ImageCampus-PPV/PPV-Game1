using System;

public abstract class Item : BaseEntity
{
}

public abstract class DragonItem : Item
{
}

public abstract class MechaItem : Item
{
}

public class Ferrite : MechaItem
{
}

public class Ancestral : DragonItem
{
}

public abstract class DragonInteractable : Interactable
{

}

public abstract class MechaInteractable : Interactable
{

}

public class MineOfAttribute : Attribute
{
    public Type itemType;

    public MineOfAttribute(Type itemType)
    {
        this.itemType = itemType;
    }
}