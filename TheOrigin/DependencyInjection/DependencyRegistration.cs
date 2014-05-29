using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class DependencyRegistration : IDependencyRegistration
    {
        private Type _resolveType = null;
        private Type _resolveAsBaseClass = null;
        private Type _resolveAsInterface = null;
        private bool _resolveAsDefault = false;
        private bool _resolveAsSingleton = false;

        public DependencyRegistration(Type TypeToResolve)
        {
            this._resolveType = TypeToResolve;
        }

        public IDependencyRegistration AsInterface<TInterface>()
        {
            return this.AsInterface(typeof(TInterface));
        }

        public IDependencyRegistration AsInterface(Type InterfaceType)
        {
            // check type compatibility.
            if (!InterfaceType.IsAssignableFrom(this._resolveType))
                throw new TypeIncompatibilityException("ERROR_TYPE_INCOMPATILB_TO_INTERFACE");

            this._resolveAsInterface = InterfaceType;
            return this;
        }

        public IDependencyRegistration AsBaseClass<TBaseClass>()
        {
            return this.AsBaseClass(typeof(TBaseClass));
        }

        public IDependencyRegistration AsBaseClass(Type BaseClassType)
        {
            // check type compatibility.
            if (!(this._resolveType.IsSubclassOf(BaseClassType)))
                throw new TypeIncompatibilityException("ERROR_TYPE_INCOMPATILB_TO_BASECLASS");

            this._resolveAsBaseClass = BaseClassType;
            return this;
        }

        public IDependencyRegistration AsDefault()
        {
            this._resolveAsDefault = true;
            return this;
        }

        public IDependencyRegistration AsSingleton()
        {
            this._resolveAsSingleton = true;
            return this;
        }

        public Type GetRegisteredType()
        {
            return this._resolveType;
        }

        public Type GetRegisteredInterfaceType()
        {
            return this._resolveAsInterface;
        }

        public Type GetRegisteredBaseClassType()
        {
            return this._resolveAsBaseClass;
        }

        public bool GetRegisteredTypeAsDefault()
        {
            return this._resolveAsDefault;
        }

        public bool GetRegisteredTypeAsSingleton()
        {
            return this._resolveAsSingleton;
        }
    }
}
