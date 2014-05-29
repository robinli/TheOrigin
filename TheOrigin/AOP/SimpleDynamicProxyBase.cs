using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class SimpleDynamicProxyBase
    {
        public IInterceptor _interceptor = null;
        public IPropertyInterceptor _propertyInterceptor = null;
        public object _proxyInstance = null;
    }
}
