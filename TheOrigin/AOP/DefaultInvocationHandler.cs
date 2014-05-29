using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultInvocationHandler : IInvocationHandler
    {
        public MethodInfo Method
        {
            get { throw new NotImplementedException(); }
        }

        public void Proceed()
        {
            throw new NotImplementedException();
        }

        public object ReturnValue
        {
            get { throw new NotImplementedException(); }
        }
    }
}
