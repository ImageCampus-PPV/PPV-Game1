
namespace GreenAbis
{
    public sealed class Resource
    {
        private string name;
        private long minValue;
        private long maxValue;
        private long currentValue;

        public string Name => name;
        public long CurrentValue => currentValue;

        public Resource()
        {
        }

        public Resource(string name, long minValue, long maxValue, long startValue)
        {
            this.name = name;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = System.Math.Clamp(startValue, minValue, maxValue);
        }

        public void AddResource(long amount)
        {
            currentValue = System.Math.Clamp(currentValue + amount, minValue, maxValue);
        }

        public void RemoveResource(long amount)
        {
            currentValue = System.Math.Clamp(currentValue - amount, minValue, maxValue);
        }

        public void SetResourceAmount(long amount)
        {
            currentValue = System.Math.Clamp(amount, minValue, maxValue);
        }
    }
}
