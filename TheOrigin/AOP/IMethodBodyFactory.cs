using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public interface IMethodBodyFactory
    {
        void ImplementBody(ILGenerator BodyGenerator, params object[] BodyParams);
    }
}
