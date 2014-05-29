using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP.MethodBodyImplFactories
{
    public class DefaultConstructorMethodBodyFactory : IMethodBodyFactory
    {
        public void ImplementBody(ILGenerator BodyGenerator, params object[] BodyParams)
        {
            BodyGenerator.Emit(OpCodes.Nop);
            BodyGenerator.Emit(OpCodes.Ret);
        }
    }
}
