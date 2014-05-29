using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class ProxyRegistration
    {
        private Type _proxyType = null;
        private Type _proxyConcreteType = null;
        private ISimpleDynamicProxyService _proxyService = null;
        private List<IInterceptor> _interceptors = null;
        private List<IPropertyInterceptor> _propertyInterceptors = null;

        public ProxyRegistration()
        {
            this._interceptors = new List<IInterceptor>();
            this._propertyInterceptors = new List<IPropertyInterceptor>();
        }

        public ProxyRegistration For<T>()
        {
            return this.For(typeof(T));
        }

        public ProxyRegistration For(Type TypeToProxy)
        {
            this._proxyType = TypeToProxy;
            return this;
        }

        public ProxyRegistration Implemented<T>()
        {
            return this.Implemented(typeof(T));
        }

        public ProxyRegistration Implemented(Type TypeConcrete)
        {
            if (this._proxyType.IsInterface &&
                TypeConcrete.GetInterfaces().Where(i => i.FullName == this._proxyType.FullName).Count() == 0)
                throw new InvalidOperationException("ERROR_CONCRETE_CLASS_NOT_IMPLEMENT_GIVEN_INTERFACE");
            if (this._proxyType.IsClass && TypeConcrete.BaseType != this._proxyType)
                throw new InvalidOperationException("ERROR_CONCRETE_CLASS_NOT_INHERT_FROM_GIVEN_BASE_CLASS");

            this._proxyConcreteType = TypeConcrete;
            return this;
        }

        public ProxyRegistration RegisterInterceptor(IInterceptor Interceptor)
        {
            this._interceptors.Add(Interceptor);
            return this;
        }

        public ProxyRegistration RegisterPropertyInterceptor(IPropertyInterceptor Interceptor)
        {
            this._propertyInterceptors.Add(Interceptor);
            return this;
        }

        //public PropertyInterceptorRegistration InterceptProperty<T>(
        //    Expression<Func<T, object>> PropertySelector, IPropertyInterceptor PropertyInterceptor)
        //{
        //    var propName = Utils.GetMapPropertyName<T>(PropertySelector);
        //    var property = this._proxyType.GetProperty(propName);

        //    if (string.IsNullOrEmpty(propName))
        //        throw new InvalidOperationException("ERROR_PROPERTY_NOT_FOUND");

        //    var registration = new PropertyInterceptorRegistration(property, PropertyInterceptor);
        //    this._propertyInterceptorRegistrations.Add(property, registration);
        //    return registration;
        //}

        //public MethodInterceptorRegistration InterceptMethod<T>(MethodSelector Selector)
        //{
        //    var method = Selector.GetMethodInfo();

        //    if (method == null)
        //        throw new InvalidOperationException("ERROR_METHOD_NOT_FOUND");

        //    var methodRegistration = new MethodInterceptorRegistration(method);
        //    this._methodInterceptorRegistrations.Add(method, methodRegistration);
        //    return methodRegistration;
        //}

        //public IDictionary<PropertyInfo, PropertyInterceptorRegistration> GetPropertyInterceptorRegistrations()
        //{
        //    return this._propertyInterceptorRegistrations;
        //}

        public IList<IInterceptor> GetInterceptors()
        {
            return this._interceptors;
        }

        public IList<IPropertyInterceptor> GetPropertyInterceptors()
        {
            return this._propertyInterceptors;
        }

        public T CreateProxy<T>()
        {
            if (typeof(T) != this._proxyType)
                throw new InvalidOperationException("ERROR_ILLEGAL_TYPE_CASTING");

            return (T)this.CreateProxy();
        }

        public T CreateProxy<T>(object ProxyInstance)
        {
            if (typeof(T) != this._proxyType)
                throw new InvalidOperationException("ERROR_ILLEGAL_TYPE_CASTING");

            return (T)this.CreateProxy(ProxyInstance);
        }

        public object CreateProxy()
        {
            if (this._proxyType.IsInterface && this._proxyConcreteType == null)
                throw new InvalidOperationException("ERROR_INTERFACE_NEEDS_CONCRETE_TYPE_TO_INSTANTIATE");

            return this.CreateProxy(Activator.CreateInstance(this._proxyConcreteType));
        }

        public object CreateProxy(object ProxyInstance)
        {
            if (this._proxyService == null)
                this._proxyService = AOPServiceProvider.Resolve<ISimpleDynamicProxyService>(this._proxyType);

            return this._proxyService.CreateProxy(this.GetInterceptors().First(), this.GetPropertyInterceptors().First(), ProxyInstance);
        }
    }
}
