using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultConstructorFactory : IConstructorFactory
    {
        private TypeBuilder _typeBuilder = null;
        private ConstructorBuilder _constructorBuilder = null;
        private List<Type> _constructorParams = new List<Type>();
        private MethodAttributes _attributes = MethodAttributes.Public | MethodAttributes.SpecialName;
        private CallingConventions _convention = CallingConventions.Standard;
        private Action<ILGenerator> _implementation = null;

        public DefaultConstructorFactory(TypeBuilder Builder)
        {
            this._typeBuilder = Builder;
        }

        public IConstructorFactory Attribute(MethodAttributes Attributes)
        {
            this._attributes = Attributes;
            return this;
        }

        public IConstructorFactory CallingConvention(CallingConventions Convention)
        {
            this._convention = Convention;
            return this;
        }
        public IConstructorFactory WithParameter<T>()
        {
            this._constructorParams.Add(typeof(T));
            return this;
        }

        public IConstructorFactory WithParameter(Type ParameterType)
        {
            this._constructorParams.Add(ParameterType);
            return this;
        }

        public IConstructorFactory ImplementBody(Action<ILGenerator> ImplementBodyExpression)
        {
            this._implementation = ImplementBodyExpression;
            return this;
        }

        public void Generate()
        {
            if (this._constructorParams != null && this._constructorParams.Count > 0)
                this._constructorBuilder = this._typeBuilder.DefineConstructor(
                    this._attributes, this._convention, this._constructorParams.ToArray());
            else
                this._constructorBuilder = this._typeBuilder.DefineConstructor(
                    this._attributes, this._convention, Type.EmptyTypes);

            this._implementation(this._constructorBuilder.GetILGenerator());
        }

        public ConstructorBuilder GetConstructorBuilder()
        {
            if (this._constructorBuilder == null)
                this.Generate();

            return this._constructorBuilder;
        }
    }
}
