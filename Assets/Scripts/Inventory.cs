using ImageCampus.ToolBox.Services;

public class Inventory : IService
{
    public bool IsPersistance => false;

    public const uint MaxSpace = 50;
}
