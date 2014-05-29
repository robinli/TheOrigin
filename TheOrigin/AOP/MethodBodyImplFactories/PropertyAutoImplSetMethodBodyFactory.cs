using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP.MethodBodyImplFactories
{
    public class PropertyAutoImplSetMethodBodyFactory : IMethodBodyFactory
    {
        public void ImplementBody(ILGenerator BodyGenerator, params object[] BodyParams)
        {
            if (BodyParams == null || BodyParams.Length == 0)
                throw new ArgumentException("ERROR_BODY_ARGUMENT_NOT_FOUND");
            if (!(BodyParams[0] is FieldBuilder))
                throw new ArgumentException("ERROR_BODY_ARGUMENT_TYPE_IS_NOT_EXPECTED");
            
            BodyGenerator.Emit(OpCodes.Ldarg_0);
            BodyGenerator.Emit(OpCodes.Ldarg_1);
            BodyGenerator.Emit(OpCodes.Stfld, (BodyParams[0] as FieldBuilder));
            BodyGenerator.Emit(OpCodes.Nop);
            BodyGenerator.Emit(OpCodes.Ret);
        }
    }
}
