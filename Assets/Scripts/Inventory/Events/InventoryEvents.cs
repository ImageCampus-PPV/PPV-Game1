using ImageCampus.ToolBox.Events;

namespace Inventory.Events
{
    public struct ItemAddedEvent : IEvent
    {
        public Item Item { get; private set; }
        public MyInventory Owner { get; private set; }

        public void Assign(params object[] parameters)
        {
            Item = (Item)parameters[0];
            Owner = (MyInventory)parameters[1];
        }

        public void Reset()
        {
            Item = null;
            Owner = null;
        }
    }

    public struct ItemRemovedEvent : IEvent
    {
        public Item Item { get; private set; }
        public MyInventory Owner { get; private set; }

        public void Assign(params object[] parameters)
        {
            Item = (Item)parameters[0];
            Owner = (MyInventory)parameters[1];
        }

        public void Reset()
        {
            Item = null;
            Owner = null;
        }
    }
}
