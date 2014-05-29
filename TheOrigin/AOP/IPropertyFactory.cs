using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IPropertyFactory
    {
        // configuration.
        IPropertyFactory Attribute(PropertyAttributes Attributes);
        IPropertyFactory Mode(PropertyModes Mode);
        IPropertyFactory WithParameter<T>();
        IPropertyFactory WithParameter(Type ParameterType);
        IPropertyFactory AutoImplemented();
        IPropertyFactory ForImplementFromInterface();
        IPropertyFactory ImplementGetBody(Action<ILGenerator> ImplementBodyExpression, params object[] GetBodyParams);
        IPropertyFactory ImplementSetBody(Action<ILGenerator> ImplementBodyExpression, params object[] SetBodyParams);

        // service.
        void Generate();
        IMethodFactory GetPropertyReadMethod();
        IMethodFactory GetPropertyWriteMethod();
    }
}
