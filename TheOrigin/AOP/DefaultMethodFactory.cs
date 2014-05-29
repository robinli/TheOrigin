using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultMethodFactory : IMethodFactory
    {
        private MethodBuilder _methodBuilder = null;
        private TypeBuilder _typeBuilder = null;
        private string _methodName = null;
        private MethodAttributes _methodAttributes = 
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot;
        private Type _returnValueType = null;
        private CallingConventions _methodCallingConventions = CallingConventions.Standard;
        private List<Type> _methodParameterTypes = new List<Type>();
        private Action<ILGenerator, object[]> _implementation = null;
        private object[] _bodyParams = null;

        public DefaultMethodFactory(TypeBuilder Builder, string MethodName)
        {
            this._typeBuilder = Builder;
            this._methodName = MethodName;
        }

        public IMethodFactory WithReturn<T>()
        {
            this._returnValueType = typeof(T);
            return this;
        }

        public IMethodFactory WithReturn(Type ReturnType)
        {
            this._returnValueType = ReturnType;
            return this;
        }

        public IMethodFactory WithParameter<T>()
        {
            this._methodParameterTypes.Add(typeof(T));
            return this;
        }

        public IMethodFactory WithParameter(Type ParameterType)
        {
            this._methodParameterTypes.Add(ParameterType);
            return this;
        }

        public IMethodFactory ForCallingConventions(CallingConventions Conventions)
        {
            this._methodCallingConventions = Conventions;
            return this;
        }

        public IMethodFactory ForImplementFromInterface()
        {
            this._methodAttributes = 
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final |
                MethodAttributes.ReuseSlot | MethodAttributes.Virtual;
            return this;
        }

        public IMethodFactory ImplementBody(Action<ILGenerator, object[]> ImplementBodyExpression, params object[] BodyParams)
        {
            this._implementation = ImplementBodyExpression;
            this._bodyParams = BodyParams;
            return this;
        }

        public ILGenerator GetMethodBodyGenerator()
        {
            if (this._methodBuilder == null)
                this.Generate();

            return this._methodBuilder.GetILGenerator();
        }

        public MethodBuilder GetMethodBuilder()
        {
            return this._methodBuilder;
        }

        public void Generate()
        {
            this._methodBuilder = this._typeBuilder.DefineMethod(
                this._methodName, this._methodAttributes, this._methodCallingConventions, 
                this._returnValueType, (this._methodParameterTypes.Count == 0) ? Type.EmptyTypes : this._methodParameterTypes.ToArray());

            this._implementation(this._methodBuilder.GetILGenerator(), this._bodyParams);
        }
    }
}
