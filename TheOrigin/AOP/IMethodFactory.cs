using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IMethodFactory
    {
        // configuration.
        IMethodFactory WithReturn<T>();
        IMethodFactory WithReturn(Type ReturnType);
        IMethodFactory WithParameter<T>();
        IMethodFactory WithParameter(Type ParameterType);
        IMethodFactory ForImplementFromInterface();
        IMethodFactory ForCallingConventions(CallingConventions Conventions);
        IMethodFactory ImplementBody(Action<ILGenerator, object[]> ImplementBodyExpression, params object[] BodyParams);

        // service.
        void Generate();
        MethodBuilder GetMethodBuilder();
    }
}
