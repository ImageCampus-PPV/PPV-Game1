
namespace GreenAbis
{
    public sealed class Resource
    {
        private string name;
        private uint minValue;
        private uint maxValue;
        private uint currentValue;

        public string Name => name;
        public uint CurrentValue => currentValue;

        public Resource()
        {
        }

        public Resource(string name, uint minValue, uint maxValue, uint startValue)
        {
            this.name = name;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = System.Math.Clamp(startValue, minValue, maxValue);
        }

        public void AddResource(uint amount)
        {
            currentValue = System.Math.Clamp(currentValue + amount, minValue, maxValue);
        }

        public void RemoveResource(uint amount)
        {
            currentValue = System.Math.Clamp(currentValue - amount, minValue, maxValue);
        }

        public void SetResourceAmount(uint amount)
        {
            currentValue = System.Math.Clamp(amount, minValue, maxValue);
        }
    }
}
