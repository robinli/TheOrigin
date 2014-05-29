using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class ContainerBuilder : IContainerBuilder
    {
        private List<IDependencyRegistration> _typeRegistered = null;
        private Type _serviceContainerType = typeof(Container);
        private Type _dependencyRegistrationType = typeof(DependencyRegistration);

        public ContainerBuilder()
        {
            this._typeRegistered = new List<IDependencyRegistration>();
        }

        public IDependencyRegistration Register<TypeToRegister>()
        {
            return this.Register(typeof(TypeToRegister));
        }

        public IDependencyRegistration Register(Type TypeToRegister)
        {
            if (!TypeToRegister.IsClass)
                throw new ArgumentException("ERROR_IMPLEMENTATION_TYPE_IS_NOT_CLASS");

            var componentRegistration = this.GetComponentRegistration(TypeToRegister);
            this._typeRegistered.Add(componentRegistration);
            return componentRegistration;
        }

        public IContainer GetContainer()
        {
            return Container.GetContainer(this._typeRegistered);
        }

        private void RegisterComponentRegistration(IDependencyRegistration Registration)
        {
            var registrationQuery = this._typeRegistered.Where(
                c => c.GetRegisteredType() == Registration.GetRegisteredType());

            if (registrationQuery.Any())
            {
                this._typeRegistered.Remove(registrationQuery.First());
                this._typeRegistered.Add(Registration);
            }
            else
                this._typeRegistered.Add(Registration);
        }

        private IDependencyRegistration GetComponentRegistration(Type TypeToRegister)
        {
            return (IDependencyRegistration)Activator.CreateInstance(
                this._dependencyRegistrationType, new object[] { TypeToRegister });
        }
    }
}
