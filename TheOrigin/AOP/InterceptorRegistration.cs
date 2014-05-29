using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class InterceptorRegistration : IInterceptorRegistration
    {
        private Type InterceptorProxyType { get; set; }
        private Dictionary<PropertyInfo, List<IPropertyInterceptor>> PropertyInterceptorRegistrations { get; set; }
        private Dictionary<MethodInfo, List<IInterceptor>> MethodInterceptorRegistrations { get; set; }
        
        private InterceptorRegistration(Type InterceptorProxyType)
        {
            this.InterceptorProxyType = InterceptorProxyType;
            this.MethodInterceptorRegistrations = new Dictionary<MethodInfo, List<IInterceptor>>();
            this.PropertyInterceptorRegistrations = new Dictionary<PropertyInfo, List<IPropertyInterceptor>>();
        }

        internal static InterceptorRegistration For<T>() 
        {
            return InterceptorRegistration.For(typeof(T));
        }

        internal static InterceptorRegistration For(Type InterceptorProxyType)
        {
            if (!InterceptorProxyType.IsInterface)
                throw new ArgumentException("ERROR_AOP_PROXY_TYPE_ALLOWED_INTERFACE_ONLY");

            var registration = new InterceptorRegistration(InterceptorProxyType);
            return registration;
        }

        public IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor)
        {
            var propertyInfo = this.InterceptorProxyType.GetProperty(PropertyName);
            var propQuery = this.PropertyInterceptorRegistrations.Where(c => c.Key == propertyInfo);

            if (propQuery.Any())
                this.PropertyInterceptorRegistrations[propertyInfo].Add(Interceptor);
            else
                this.PropertyInterceptorRegistrations.Add(propertyInfo, new List<IPropertyInterceptor>() { Interceptor });

            return this;
        }

        public IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor, int Order)
        {
            var propertyInfo = this.InterceptorProxyType.GetProperty(PropertyName);
            var propQuery = this.PropertyInterceptorRegistrations.Where(c => c.Key == propertyInfo);

            if (propQuery.Any())
                this.PropertyInterceptorRegistrations[propertyInfo].Insert(Order, Interceptor);
            else
                this.PropertyInterceptorRegistrations.Add(propertyInfo, new List<IPropertyInterceptor>() { Interceptor });

            return this;
        }

        public IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor)
        {
            var methodInfo = this.InterceptorProxyType.GetMethod(MethodName);
            var methodQuery = this.MethodInterceptorRegistrations.Where(c => c.Key == methodInfo);

            if (methodQuery.Any())
                this.MethodInterceptorRegistrations[methodInfo].Add(Interceptor);
            else
                this.MethodInterceptorRegistrations.Add(methodInfo, new List<IInterceptor>() { Interceptor });

            return this;
        }

        public IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor, int Order)
        {
            var methodInfo = this.InterceptorProxyType.GetMethod(MethodName);
            var methodQuery = this.MethodInterceptorRegistrations.Where(c => c.Key == methodInfo);

            if (methodQuery.Any())
                this.MethodInterceptorRegistrations[methodInfo].Insert(Order, Interceptor);
            else
                this.MethodInterceptorRegistrations.Add(methodInfo, new List<IInterceptor>() { Interceptor });

            return this;
        }

        public IList<IPropertyInterceptor> GetPropertyInterceptors(string PropertyName)
        {
            var propertyInfo = this.InterceptorProxyType.GetProperty(PropertyName);
            var propQuery = this.PropertyInterceptorRegistrations.Where(c => c.Key == propertyInfo);

            if (propQuery.Any())
                return this.PropertyInterceptorRegistrations[propertyInfo];
            else
                return null;
        }

        public IList<IInterceptor> GetMethodInterceptors(string MethodName)
        {
            var methodInfo = this.InterceptorProxyType.GetMethod(MethodName);
            var methodQuery = this.MethodInterceptorRegistrations.Where(c => c.Key == methodInfo);

            if (methodQuery.Any())
                return this.MethodInterceptorRegistrations[methodInfo];
            else
                return null;
        }
    }
}
