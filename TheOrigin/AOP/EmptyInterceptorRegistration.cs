using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public class EmptyInterceptorRegistration : IInterceptorRegistration
    {
        // identify the condition only, no any member required.
        public IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor)
        {
            throw new NotImplementedException();
        }

        public IInterceptorRegistration PropertyIntercept(string PropertyName, IPropertyInterceptor Interceptor, int Order)
        {
            throw new NotImplementedException();
        }

        public IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor)
        {
            throw new NotImplementedException();
        }

        public IInterceptorRegistration MethodIntercept(string MethodName, IInterceptor Interceptor, int Order)
        {
            throw new NotImplementedException();
        }

        public IList<IPropertyInterceptor> GetPropertyInterceptors(string PropertyName)
        {
            throw new NotImplementedException();
        }

        public IList<IInterceptor> GetMethodInterceptors(string MethodName)
        {
            throw new NotImplementedException();
        }
    }
}
