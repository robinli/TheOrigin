using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public class MethodSelector
    {
        private Type _typeSearch = null;
        private string _name = null;
        private Type[] _parameterTypes = Type.EmptyTypes;

        public MethodSelector(Type TypeToSearch, string Name)
        {
            this._typeSearch = TypeToSearch;
            this._name = Name;
        }

        public MethodSelector(Type TypeToSearch, string Name, params Type[] Types) : this(TypeToSearch, Name)
        {
            this._typeSearch = TypeToSearch;
            this._parameterTypes = Types;
        }

        public MethodInfo GetMethodInfo()
        {
            if (this._parameterTypes == null || this._parameterTypes.Length == 0)
                return this._typeSearch.GetMethod(this._name);
            else
                return this._typeSearch.GetMethod(this._name, this._parameterTypes);
        }
    }
}
