using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class ProxyInvocation
    {
        private string _invocationTargetName = null;
        private Dictionary<string, object> _invocationParameters = new Dictionary<string, object>();

        public ProxyInvocation(string TargetName, Dictionary<string, object> Parameters)
        {
            this._invocationTargetName = TargetName;
            this._invocationParameters = Parameters;
        }

        public object Proceed()
        {
            return null;
        }
    }
}
