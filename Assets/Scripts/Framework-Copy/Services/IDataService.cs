namespace GreenAbyss.Services
{
	public interface IDataService : IService 
    {
        public string ServiceReference { get; }
        public object GetDataValue(string[] dataPath);
    }
}