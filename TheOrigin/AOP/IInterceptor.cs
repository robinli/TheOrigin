using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public interface IInterceptor
    {
        void BeforeExecute(string MethodName, Dictionary<string, object> InvokeArgs);
        void AfterExecute(string MethodName, Dictionary<string, object> InvokeArgs, object ReturnValue);
    }
}
