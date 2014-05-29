using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public interface IDependencyRegistration
    {
        IDependencyRegistration AsInterface<TInterface>();
        IDependencyRegistration AsInterface(Type InterfaceType);
        IDependencyRegistration AsBaseClass<TBaseClass>();
        IDependencyRegistration AsBaseClass(Type BaseClassType);
        IDependencyRegistration AsDefault();
        IDependencyRegistration AsSingleton();
        //IDependencyRegistration Named(string Name);

        Type GetRegisteredType();
        Type GetRegisteredInterfaceType();
        Type GetRegisteredBaseClassType();
        bool GetRegisteredTypeAsDefault();
        bool GetRegisteredTypeAsSingleton();
        //string GetName();
    }
}
