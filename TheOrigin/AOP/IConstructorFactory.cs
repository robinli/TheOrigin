using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public interface IConstructorFactory
    {
        // configuration.
        IConstructorFactory Attribute(MethodAttributes Attributes);
        IConstructorFactory CallingConvention(CallingConventions Convention);
        IConstructorFactory WithParameter<T>();
        IConstructorFactory WithParameter(Type ParameterType);
        IConstructorFactory ImplementBody(Action<ILGenerator> ImplementBodyExpression);

        // service.
        void Generate();
        ConstructorBuilder GetConstructorBuilder();
    }
}
