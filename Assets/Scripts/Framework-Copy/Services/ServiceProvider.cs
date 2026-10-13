using System;
using System.Collections.Generic;

namespace GreenAbyss.Services
{
    public sealed class ServiceProvider
    {
        private static ServiceProvider _instance;
        public static ServiceProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceProvider();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private readonly Dictionary<string, Type> _dataServiceTypeByReference = new Dictionary<string, Type>();
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        private ServiceProvider() { }

        public void AddService<ServiceType>(IService service) where ServiceType : class, IService
        {
            if (!_services.ContainsKey(typeof(ServiceType)))
            { 
                _services.Add(typeof(ServiceType), service);
				if (typeof(IDataService).IsAssignableFrom(typeof(ServiceType)))
                    _dataServiceTypeByReference.Add((service as IDataService).ServiceReference, typeof(ServiceType));
            }
        }

        public bool RemoveService<ServiceType>() where ServiceType : class, IService
        {
            if (!_services.ContainsKey(typeof(ServiceType)))
                throw new KeyNotFoundException();
            return _services.Remove(typeof(ServiceType));
        }

        public bool ContainsService<ServiceType>() where ServiceType : class, IService
        {
            return _services.ContainsKey(typeof(ServiceType));
        }

        public ServiceType GetService<ServiceType>() where ServiceType : class, IService
        {
            return _services[typeof(ServiceType)] as ServiceType;
        }

        public IDataService GetDataService(string serviceReference) 
        {
			if (!_dataServiceTypeByReference.ContainsKey(serviceReference))
                return null;
            return _services[_dataServiceTypeByReference[serviceReference]] as IDataService;
        }

        public void ClearAllServices()
        {
            _services.Clear();
            _dataServiceTypeByReference.Clear();
        }

        public void ClearAllNonPersistanceServices() 
        {
            List<Type> nonPersistanceServiceTypes = new List<Type>();
            foreach (KeyValuePair<Type, IService> service in _services)
            {
                if (!service.Value.IsPersistance)
                    nonPersistanceServiceTypes.Add(service.Key);
            }
            foreach (Type keyToRemove in nonPersistanceServiceTypes)
            {
				if (typeof(IDataService).IsAssignableFrom(keyToRemove))
                    _dataServiceTypeByReference.Remove((_services[keyToRemove] as IDataService).ServiceReference);
                _services.Remove(keyToRemove);
            }
        }
    }
}
