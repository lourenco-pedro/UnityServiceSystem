using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Services
{
    public static class ServiceContainer
    {
        static string ERROR_NotInitializedOrImplemented => "{0} is not implemented or not initialized in ServiceContainer";
        static string ERROR_MissingImplementation => "{0} does not implement {1}"; 
        
        static Dictionary<string, IService> _services = new Dictionary<string, IService>();

        public delegate void CallbackEventHandler();

        public static async Task<TServiceModel> AddService<TServiceModel, TImplementation>()
            where TServiceModel : IService
            where TImplementation : TServiceModel
        {
            TImplementation instance = (TImplementation)Activator.CreateInstance(typeof(TImplementation));
            Type instanceType = instance.GetType();

            MethodInfo setupMethod = instanceType.GetMethod("Setup");
            MethodInfo asyncSetupMethod = instanceType.GetMethod("AsyncSetup");
            if(null != setupMethod)
                setupMethod.Invoke(instance, null);
            else if(null != asyncSetupMethod)
                await (Task)asyncSetupMethod.Invoke(instance, null);
            
            _services.Add(typeof(TServiceModel).Name, (TServiceModel)instance);

            return instance;
        }

        public static async Task<object> AddService(Type targetInterface, Type implementation)
        {
            object instance = Activator.CreateInstance(implementation);
            Type instanceType = instance.GetType();
            
            MethodInfo setupMethod = instanceType.GetMethod("Setup");
            MethodInfo asyncSetupMethod = instanceType.GetMethod("AsyncSetup");
            if(null != setupMethod)
                setupMethod.Invoke(instance, null);
            else if(null != asyncSetupMethod)
                await (Task)asyncSetupMethod.Invoke(instance, null);

            Debug.Log("Added " + targetInterface.Name);
            
            _services.Add(targetInterface.Name, (IService)instance);
            return instance;
        }

        public static void UseService<TServiceModel>(Action<TServiceModel> onServiceLoad)
            where TServiceModel: IService
        {
            string name = typeof(TServiceModel).Name;
            
            if(!_services.ContainsKey(name))
                throw new NotImplementedException(string.Format(ERROR_NotInitializedOrImplemented, name));
            
            onServiceLoad.Invoke((TServiceModel)_services[name]);
        }

        public static void UseServices<TService1, TService2>(Action<TService1, TService2> onServicesLoad)
            where TService1 : IService
            where TService2 : IService
        {
            List<IService> foundServices = new List<IService>();
            
            UseService<TService1>(service => foundServices.Add(service));
            UseService<TService2>(service => foundServices.Add(service));

            onServicesLoad.Invoke((TService1)foundServices[0], (TService2)foundServices[1]);
        }
        public static void UseServices<TService1, TService2, TService3>(Action<TService1, TService2, TService3> onServicesLoad)
            where TService1 : IService
            where TService2 : IService
            where TService3 : IService
        {
            List<IService> foundServices = new List<IService>();

            UseServices<TService1, TService2>((service1, service2) =>
            {
                foundServices.Add(service1);
                foundServices.Add(service2);
            });
            UseService<TService3>(service3 => { foundServices.Add(service3); });
            

            onServicesLoad.Invoke((TService1)foundServices[0], (TService2)foundServices[1], (TService3)foundServices[2]);
        }
        
        public static void UseServices<TService1, TService2, TService3, TService4>(Action<TService1, TService2, TService3, TService4> onServicesLoad)
            where TService1 : IService
            where TService2 : IService
            where TService3 : IService
            where TService4 : IService
        {
            List<IService> foundServices = new List<IService>();

            UseServices<TService1, TService2, TService3>((service1, service2, service3) =>
            {
                foundServices.Add(service1);
                foundServices.Add(service2);
                foundServices.Add(service3);
            });
            UseService<TService4>(service4 => { foundServices.Add(service4); });
            

            onServicesLoad.Invoke((TService1)foundServices[0], (TService2)foundServices[1], (TService3)foundServices[2], (TService4)foundServices[3]);
        }

        public static void UseService(Type service, Action<IService> onServiceLoad)
        {
            string name = service.Name;
            try
            {
                onServiceLoad.Invoke(_services[name]);
            }
            catch
            {
                throw new NotImplementedException(string.Format(ERROR_NotInitializedOrImplemented, name));
            }
        }
        
#if UNITY_EDITOR
        [System.Obsolete("You should not be using this function. It's only for the debug window. It will not be included in the final build")]
        public static void EDITOR_DrawServiceSelector(Action<int> onService)
        {
            foreach (var service in _services)
            {
                onService(service.Key.GetHashCode());
            }
        }

        [System.Obsolete("You should not be using this function. It's only for the debug window. It will not be included in the final build")]
        public static IService EDITOR_GetServiceByHash(int hash)
        {  
            return _services.First(service => service.Key.GetHashCode() == hash).Value;
        }
#endif
    }

    internal static class InterfaceUtils
    {
        internal static bool Implements<TImplementation, TService>()
        {
            return typeof(TService).IsAssignableFrom(typeof(TImplementation));
        }
    }
}