using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IInterceptorRegistration
    {
        IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor);
        IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor, int Order);
        IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor);
        IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor, int Order);
        IList<IPropertyInterceptor> GetPropertyInterceptors(string PropertyName);
        IList<IInterceptor> GetMethodInterceptors(string MethodName);
    }
}
