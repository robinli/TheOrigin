using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public static class InterceptorRegistrationService
    {
        private static Dictionary<Type, InterceptorRegistration> TypeInterceptorRegistrations = new Dictionary<Type, InterceptorRegistration>();
        
        public static IInterceptorRegistration For<T>() 
        {
            return For(typeof(T));
        }

        public static IInterceptorRegistration For(Type ConcreteTypeProxy)
        {
            if (!ConcreteTypeProxy.IsInterface)
                throw new ArgumentException("ERROR_AOP_PROXY_TYPE_ALLOWED_INTERFACE_ONLY");

            var registration = InterceptorRegistration.For(ConcreteTypeProxy);

            if (TypeInterceptorRegistrations.ContainsKey(ConcreteTypeProxy))
                TypeInterceptorRegistrations[ConcreteTypeProxy] = registration;
            else
                TypeInterceptorRegistrations.Add(ConcreteTypeProxy, registration);

            return registration;
        }

        public static IInterceptorRegistration Resolve<T>() 
        {
            return Resolve(typeof(T));
        }

        public static IInterceptorRegistration Resolve(Type ConcreteTypeProxy)
        {
            if (!TypeInterceptorRegistrations.ContainsKey(ConcreteTypeProxy))
                return new EmptyInterceptorRegistration();
            else
                return TypeInterceptorRegistrations[ConcreteTypeProxy];
        }
    }
}
