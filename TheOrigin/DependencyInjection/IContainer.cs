using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public interface IContainer
    {
        TInterfaceType Resolve<TInterfaceType>();
        object Resolve(Type TypeToResolve);
        TInterfaceType Resolve<TInterfaceType, TConcreteType>();
        object Resolve(Type TypeToResolve, Type ConcreteType);
        TInterfaceType Resolve<TInterfaceType>(IEnumerable<object> InstantiateParameters);
        object Resolve(Type TypeToResolve, IEnumerable<object> InstantiateParameters);
        TInterfaceType Resolve<TInterfaceType, TConcreteType>(IEnumerable<object> InstantiateParameters);
        object Resolve(Type TypeToResolve, Type ConcreteType, IEnumerable<object> InstantiateParameters);
    }
}
