using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DynamicProxyBase
    {
        public Dictionary<MethodInfo, IList<IInterceptor>> MethodInterceptorRegistrations = null;
        public Dictionary<PropertyInfo, IList<IPropertyInterceptor>> PropertyInterceptorRegistrations = null;
        public object ConcreteObjectProxy = null;

        public DynamicProxyBase(Type ProxyType, object ConcreteObjectProxy)
        {
            this.ConcreteObjectProxy = ConcreteObjectProxy;
            this.MethodInterceptorRegistrations = new Dictionary<MethodInfo, IList<IInterceptor>>();
            this.PropertyInterceptorRegistrations = new Dictionary<PropertyInfo, IList<IPropertyInterceptor>>();

            // read interceptor registrations from Registration Service.
            var registraton = InterceptorRegistrationService.Resolve(ProxyType);

            if (!(registraton is EmptyInterceptorRegistration))
            {
                var properties = ProxyType.GetProperties();
                var methods = ProxyType.GetMethods();

                foreach (var prop in properties)
                {
                    var propInterceptors = registraton.GetPropertyInterceptors(prop.Name);

                    if (propInterceptors != null)
                        this.PropertyInterceptorRegistrations.Add(prop, propInterceptors);
                }

                foreach (var method in methods)
                {
                    var methodInterceptors = registraton.GetMethodInterceptors(method.Name);

                    if (methodInterceptors != null)
                        this.MethodInterceptorRegistrations.Add(method, methodInterceptors);
                }
            }
        }

        protected IList<IPropertyInterceptor> GetPropertyInterceptors(PropertyInfo Property)
        {
            if (this.PropertyInterceptorRegistrations.ContainsKey(Property))
                return this.PropertyInterceptorRegistrations[Property];
            else
                return null;
        }

        protected IList<IInterceptor> GetMethodInterceptors(MethodInfo Method)
        {
            if (this.MethodInterceptorRegistrations.ContainsKey(Method))
                return this.MethodInterceptorRegistrations[Method];
            else
                return null;
        }
    }
}
