﻿using TheOrigin.Framework.AOP.MethodBodyImplFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultTypeFactory : ITypeFactory
    {
        private ModuleBuilder _moduleBuilder = null;
        private TypeBuilder _typeBuilder = null;
        private bool _autoGenerateDefaultConstructor = false;
        private string _typeName = null;
        private TypeAttributes _typeAttributes = TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.AutoLayout;
        private Type _typeParent = null;
        private List<Type> _typeImplementInterfaces = new List<Type>();
        private List<IConstructorFactory> _typeConstructors = new List<IConstructorFactory>();
        private List<IPropertyFactory> _typeProperties = new List<IPropertyFactory>();
        private List<IMethodFactory> _typeMethods = new List<IMethodFactory>();

        public DefaultTypeFactory(ModuleBuilder Builder, string TypeName)
        {
            this._moduleBuilder = Builder;
            this._typeName = TypeName;
        }
        public ITypeFactory AsPublicClass()
        {
            this._typeAttributes = 
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | 
                TypeAttributes.AnsiClass | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit;
            return this;
        }

        public ITypeFactory AsInterface()
        {
            this._typeAttributes =
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.AutoLayout;
            return this;
        }

        public ITypeFactory AsTypeAttributes(TypeAttributes Attributes)
        {
            this._typeAttributes = Attributes;
            return this;
        }

        public ITypeFactory AutoGenerateDefaultConstructor()
        {
            this._autoGenerateDefaultConstructor = true;
            return this;
        }

        public ITypeFactory FromParent<T>() where T : class
        {
            this._typeParent = typeof(T);
            return this;
        }

        public ITypeFactory FromParent(Type Parent)
        {
            if (Parent.IsInterface)
                throw new ArgumentException("ERROR_INTERFACE_NOT_ALLOWED");

            this._typeParent = Parent;
            return this;
        }

        public ITypeFactory ImplementInterface<TInterface>()
        {
            if (typeof(TInterface).IsClass)
                throw new ArgumentException("ERROR_CLASS_NOT_ALLOWED");

            this._typeImplementInterfaces.Add(typeof(TInterface));
            return this;
        }

        public ITypeFactory ImplementInterface(Type Interface)
        {
            if (Interface.IsClass)
                throw new ArgumentException("ERROR_CLASS_NOT_ALLOWED");

            this._typeImplementInterfaces.Add(Interface);
            return this;
        }
        
        public IConstructorFactory CreateConstructor(params Type[] ConstructorTypes)
        {
            if (this._typeBuilder == null)
                this.Generate();

            IConstructorFactory constructor = null;

            if (ConstructorTypes == null || ConstructorTypes.Length == 0)
                constructor = AOPServiceProvider.Resolve<IConstructorFactory>(this._typeBuilder);
            else
                constructor = AOPServiceProvider.Resolve<IConstructorFactory>(this._typeBuilder, ConstructorTypes);

            this._typeConstructors.Add(constructor);
            return constructor;
        }

        public IPropertyFactory CreateProperty(string PropertyName, Type PropertyType)
        {
            if (this._typeBuilder == null)
                this.Generate();

            IPropertyFactory prop = AOPServiceProvider.Resolve<IPropertyFactory>(this._typeBuilder, PropertyName, PropertyType);
            this._typeProperties.Add(prop);
            return prop;
        }

        public IMethodFactory CreateMethod(string MethodName)
        {
            if (this._typeBuilder == null)
                this.Generate();

            IMethodFactory method = AOPServiceProvider.Resolve<IMethodFactory>(this._typeBuilder, MethodName);
            this._typeMethods.Add(method);
            return method;
        }

        public Type GetTypeFromFactory()
        {
            if (this._typeBuilder == null)
                this.Generate();

            // create default constructor
            if (this._autoGenerateDefaultConstructor)
            {
                var constructorBuilder = this._typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
                var constructorBody = constructorBuilder.GetILGenerator();
                AOPServiceProvider.Resolve<IMethodBodyFactory, DefaultConstructorMethodBodyFactory>().ImplementBody(constructorBody);
            }

            // generate registered properties
            if (this._typeConstructors.Count > 0)
            {
                foreach (var constructors in this._typeConstructors)
                    constructors.Generate();
            }

            // generate registered properties
            if (this._typeProperties.Count > 0)
            {
                foreach (var prop in this._typeProperties)
                    prop.Generate();
            }

            // generate registered methods.
            if (this._typeMethods.Count > 0)
            {
                foreach (var method in this._typeMethods)
                    method.Generate();
            }

            return this._typeBuilder.CreateType();
        }

        public void Generate()
        {
            if (this._typeParent == null)
                this._typeBuilder = this._moduleBuilder.DefineType(this._typeName, this._typeAttributes);
            else 
                this._typeBuilder = this._moduleBuilder.DefineType(this._typeName, this._typeAttributes, this._typeParent);

            if (this._typeImplementInterfaces.Count > 0)
            {
                foreach (var typeInterface in this._typeImplementInterfaces)
                    this._typeBuilder.AddInterfaceImplementation(typeInterface);
            }
        }
    }
}