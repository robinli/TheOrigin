using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public static class ProxyServiceConfiguration
    {
        private static Dictionary<Type, ProxyRegistration> _proxyRegistrations = null;

        static ProxyServiceConfiguration()
        {
            _proxyRegistrations = new Dictionary<Type, ProxyRegistration>();
        }

        public static ProxyRegistration Define<T>()
        {
            return Define(typeof(T));
        }

        public static ProxyRegistration Define(Type Type)
        {
            var reg = new ProxyRegistration().For(Type);
            _proxyRegistrations.Add(Type, reg);
            return reg;
        }

        public static ProxyRegistration GetTypeProxyRegistration(Type Type)
        {
            if (!_proxyRegistrations.ContainsKey(Type))
                return null;

            return _proxyRegistrations[Type];
        }
    }
}
