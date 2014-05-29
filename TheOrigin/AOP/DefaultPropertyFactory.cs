using TheOrigin.Framework.AOP.MethodBodyImplFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultPropertyFactory : IPropertyFactory
    {
        private bool _generateReadMethod = true, _generateWriteMethod = true, _autoImplemented = false, _implementForInterface = false;
        private string _propertyName = null;
        private Type _propertyType = null;
        private List<Type> _propertyParameterTypes = new List<Type>();
        private PropertyAttributes _propertyAttributes = PropertyAttributes.SpecialName;
        private TypeBuilder _typeBuilder = null;
        private PropertyBuilder _propertyBuilder = null;
        private FieldBuilder _propertyFieldBuilder = null;
        private IMethodFactory _propGetMethod = null;
        private IMethodFactory _propSetMethod = null;
        private Action<ILGenerator> _propGetMethodImplementation = null;
        private Action<ILGenerator> _propSetMethodImplementation = null;
        private object[] _propGetBodyParams = null, _propSetBodyParams = null;

        public DefaultPropertyFactory(TypeBuilder Builder, string PropertyName, Type PropertyType)
        {
            this._typeBuilder = Builder;
            this._propertyName = PropertyName;
            this._propertyType = PropertyType;
        }

        public IPropertyFactory Attribute(PropertyAttributes Attributes)
        {
            this._propertyAttributes = Attributes;
            return this;
        }

        public IPropertyFactory Mode(PropertyModes Mode)
        {
            this._generateReadMethod = (Mode == PropertyModes.ReadWrite || Mode == PropertyModes.ReadOnly) ? true : false;
            this._generateWriteMethod = (Mode == PropertyModes.ReadWrite || Mode == PropertyModes.WriteOnly) ? true : false;
            return this;
        }

        public IPropertyFactory WithParameter<T>()
        {
            this._propertyParameterTypes.Add(typeof(T));
            return this;
        }

        public IPropertyFactory WithParameter(Type ParameterType)
        {
            this._propertyParameterTypes.Add(ParameterType);
            return this;
        }

        public IPropertyFactory AutoImplemented()
        {
            this._autoImplemented = true;
            return this;
        }

        public IPropertyFactory ForImplementFromInterface()
        {
            this._implementForInterface = true;
            return this;
        }

        public IPropertyFactory ImplementGetBody(Action<ILGenerator> ImplementBodyExpression, params object[] GetBodyParams)
        {
            if (!this._generateReadMethod)
                return this;

            this._propGetMethodImplementation = ImplementBodyExpression;
            this._propGetBodyParams = GetBodyParams;
            return this;
        }

        public IPropertyFactory ImplementSetBody(Action<ILGenerator> ImplementBodyExpression, params object[] SetBodyParams)
        {
            if (!this._generateWriteMethod)
                return this;

            this._propSetMethodImplementation = ImplementBodyExpression;
            this._propSetBodyParams = SetBodyParams;
            return this;
        }

        public IMethodFactory GetPropertyReadMethod()
        {
            if (!this._generateReadMethod)
                return null;

            return this._propGetMethod;
        }

        public IMethodFactory GetPropertyWriteMethod()
        {
            if (!this._generateWriteMethod)
                return null;

            return this._propSetMethod;
        }

        public void Generate()
        {
            if (this._propertyParameterTypes.Count == 0)
                this._propertyBuilder = this._typeBuilder.DefineProperty(
                    this._propertyName, this._propertyAttributes, this._propertyType, null);
            else
                this._propertyBuilder = this._typeBuilder.DefineProperty(
                    this._propertyName, this._propertyAttributes, this._propertyType, this._propertyParameterTypes.ToArray());

            // create field for property store.
            this._propertyFieldBuilder = this._typeBuilder.DefineField(
                "_" + this._propertyName.ToLower(), this._propertyType, FieldAttributes.Private | FieldAttributes.SpecialName);

            // create read method.
            if (this._generateReadMethod)
            {
                this._propGetMethod = AOPServiceProvider.Resolve<IMethodFactory>(this._typeBuilder, "get_" + this._propertyName)
                    .WithReturn(this._propertyType);

                if (this._implementForInterface)
                    this._propGetMethod = this._propGetMethod.ForImplementFromInterface();

                if (this._autoImplemented)
                {
                    this._propGetMethod.ImplementBody((x, ps) =>
                        AOPServiceProvider.Resolve<IMethodBodyFactory, PropertyAutoImplGetMethodBodyFactory>()
                        .ImplementBody(x, this._propertyFieldBuilder));
                    this._propGetMethod.Generate();
                }
                else
                {
                    this._propGetMethod.ImplementBody((x, ps) =>
                    {
                        var localGetBuilder = x.DeclareLocal(this._propertyType);

                        x.Emit(OpCodes.Ldarg_0);
                        x.Emit(OpCodes.Ldfld, this._propertyFieldBuilder);
                        x.Emit(OpCodes.Stloc_0);

                        this._propGetMethodImplementation(x);

                        x.Emit(OpCodes.Ldloc_0);
                        x.Emit(OpCodes.Nop);
                        x.Emit(OpCodes.Ret);
                    });

                    this._propGetMethod.Generate();
                }

                this._propertyBuilder.SetGetMethod(this._propGetMethod.GetMethodBuilder());
            }

            // create write method.
            if (this._generateWriteMethod)
            {
                this._propSetMethod = AOPServiceProvider.Resolve<IMethodFactory>(this._typeBuilder, "set_" + this._propertyName)
                    .WithParameter(this._propertyType);

                if (this._implementForInterface)
                    this._propSetMethod = this._propSetMethod.ForImplementFromInterface();

                if (this._autoImplemented)
                {
                    this._propSetMethod.ImplementBody((x, ps) =>
                       AOPServiceProvider.Resolve<IMethodBodyFactory, PropertyAutoImplSetMethodBodyFactory>()
                       .ImplementBody(x, this._propertyFieldBuilder));
                    this._propSetMethod.Generate();
                }
                else
                {
                    this._propSetMethod.ImplementBody((x, ps) =>
                        {
                            var localSet = x.DeclareLocal(this._propertyType);
                                                        
                            // store data to field.
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_1);
                            x.Emit(OpCodes.Stfld, this._propertyFieldBuilder);
                            
                            // store new to local variable.
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldfld, this._propertyFieldBuilder);
                            x.Emit(OpCodes.Stloc_0);

                            this._propSetMethodImplementation(x);

                            x.Emit(OpCodes.Nop);
                            x.Emit(OpCodes.Ret);
                        });

                    this._propSetMethod.Generate();
                }

                this._propertyBuilder.SetSetMethod(this._propSetMethod.GetMethodBuilder());
            }
        }
    }
}
