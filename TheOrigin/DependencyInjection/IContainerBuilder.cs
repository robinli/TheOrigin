using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public interface IContainerBuilder
    {
        IDependencyRegistration Register<TConcreteType>();
        IDependencyRegistration Register(Type ConcreteType);
        IContainer GetContainer();

    }
}
