using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface ISimpleDynamicProxyService
    {
        T CreateProxy<T>(IInterceptor Interceptor, IPropertyInterceptor PropertyInterceptor, object ProxyInstance);
        object CreateProxy(IInterceptor Interceptor, IPropertyInterceptor PropertyInterceptor, object ProxyInstance);
    }
}
