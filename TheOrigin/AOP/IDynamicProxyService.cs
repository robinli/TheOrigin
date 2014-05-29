using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IDynamicProxyService
    {
        T CreateProxy<T>(object ProxyInstance);
        object CreateProxy(object ProxyInstance);
    }
}
