using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public interface IInvocationHandler
    {
        MethodInfo Method { get; }
        void Proceed();
        object ReturnValue { get; }
    }
}
