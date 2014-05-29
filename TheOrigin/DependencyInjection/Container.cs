using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class Container : IContainer
    {
        private IEnumerable<IDependencyRegistration> _typeRegistrations = null;
        private IDictionary<Type, object> _typeSingleInstanceContainer = null;
        private static object syncRoot = new object();
        private static IContainer ContainerInstance = null;

        private Container()
        {
            this._typeSingleInstanceContainer = new Dictionary<Type, object>();
        }

        private Container(IEnumerable<IDependencyRegistration> TypeRegistrations)
            : this()
        {
            if (TypeRegistrations == null)
                throw new ArgumentNullException("TypeRegistrations", "ERROR_TYPE_REGISTRATIONS_IS_MISSING");

            this._typeRegistrations = TypeRegistrations;
        }

        public static IContainer GetContainer(IEnumerable<IDependencyRegistration> TypeRegistrations)
        {
            if (Container.ContainerInstance == null)
            {
                lock (syncRoot)
                {
                    if (Container.ContainerInstance == null)
                        Container.ContainerInstance = new Container(TypeRegistrations);
                }
            }

            return Container.ContainerInstance;
        }

        public TInterfaceType Resolve<TInterfaceType>()
        {
            return (TInterfaceType)this.Resolve(typeof(TInterfaceType));
        }
        
        public TInterfaceType Resolve<TInterfaceType, TConcreteType>()
        {
            return (TInterfaceType)this.Resolve(typeof(TInterfaceType), typeof(TConcreteType));
        }

        public object Resolve(Type TypeToResolve)
        {
            var registration = this.GetComponentRegistration(TypeToResolve);

            if (registration == null)
                throw new RegistrationNotFoundException("ERROR_TYPE_REGISTRATION_NOT_FOUND");

            return this.CreateInstance(registration);
        }

        public object Resolve(Type TypeToResolve, Type ConcreteType)
        {
            var registration = this.GetComponentRegistration(TypeToResolve, ConcreteType);

            if (registration == null)
                throw new RegistrationNotFoundException("ERROR_TYPE_REGISTRATION_NOT_FOUND");

            return this.CreateInstance(registration);
        }

        public TInterfaceType Resolve<TInterfaceType>(IEnumerable<object> InstantiateParameters)
        {
            return (TInterfaceType)this.Resolve(typeof(TInterfaceType), InstantiateParameters);
        }

        public object Resolve(Type TypeToResolve, IEnumerable<object> InstantiateParameters)
        {
            var registration = this.GetComponentRegistration(TypeToResolve);

            if (registration == null)
                throw new RegistrationNotFoundException("ERROR_TYPE_REGISTRATION_NOT_FOUND");

            return this.CreateInstance(registration, InstantiateParameters);
        }

        public TInterfaceType Resolve<TInterfaceType, TConcreteType>(IEnumerable<object> InstantiateParameters)
        {
            return (TInterfaceType)this.Resolve(typeof(TInterfaceType), typeof(TConcreteType), InstantiateParameters);
        }

        public object Resolve(Type TypeToResolve, Type ConcreteType, IEnumerable<object> InstantiateParameters)
        {
            var registration = this.GetComponentRegistration(TypeToResolve, ConcreteType);

            if (registration == null)
                throw new RegistrationNotFoundException("ERROR_TYPE_REGISTRATION_NOT_FOUND");

            return this.CreateInstance(registration, InstantiateParameters);
        }

        private object CreateInstance(
            IDependencyRegistration Registration, IEnumerable<object> InstantiateParameters = null)
        {
            bool registrationAsSingleton = Registration.GetRegisteredTypeAsSingleton();
            Type registeredType = Registration.GetRegisteredType();

            if (registrationAsSingleton)
            {
                if (!this._typeSingleInstanceContainer.ContainsKey(registeredType))
                {
                    lock (syncRoot)
                    {
                        try
                        {
                            if (!this._typeSingleInstanceContainer.ContainsKey(registeredType))
                            {
                                if (InstantiateParameters == null)
                                {
                                    this._typeSingleInstanceContainer.Add(
                                        registeredType,
                                        Activator.CreateInstance(Registration.GetRegisteredType()));
                                }
                                else
                                {
                                    this._typeSingleInstanceContainer.Add(
                                        registeredType,
                                        Activator.CreateInstance(Registration.GetRegisteredType(),
                                        InstantiateParameters.ToArray()));
                                }
                            }
                        }
                        catch (MissingMethodException mme)
                        {
                            throw new TypeRegistrationException(
                                string.Format("Type '{0}' registration information is invalid: {1}", registeredType, mme.Message));
                        }
                    }
                }

                return this._typeSingleInstanceContainer[registeredType];
            }
            else
            {
                try
                {
                    if (InstantiateParameters == null)
                        return Activator.CreateInstance(registeredType);
                    else
                        return Activator.CreateInstance(registeredType, InstantiateParameters.ToArray());
                }
                catch (MissingMethodException mme)
                {
                    throw new TypeRegistrationException(
                        string.Format("Type '{0}' registration information is invalid: {1}", registeredType, mme.Message));
                }
            }
        }

        private IDependencyRegistration GetComponentRegistration(Type TypeToResolve, Type ConcreteType = null)
        {
            IEnumerable<IDependencyRegistration> registrationQuery = null;

            if (TypeToResolve.IsInterface)
            {
                if (ConcreteType == null)
                    registrationQuery = this._typeRegistrations.Where(
                        c => c.GetRegisteredInterfaceType() == TypeToResolve);
                else
                    registrationQuery = this._typeRegistrations.Where(
                        c => c.GetRegisteredInterfaceType() == TypeToResolve && c.GetRegisteredType() == ConcreteType);
            }
            else
            {
                // search base class first.
                if (ConcreteType == null)
                    registrationQuery = this._typeRegistrations.Where(
                        c => c.GetRegisteredBaseClassType() == TypeToResolve);
                else
                    registrationQuery = this._typeRegistrations.Where(
                        c => c.GetRegisteredBaseClassType() == TypeToResolve && c.GetRegisteredType() == ConcreteType);

                // if not base type, search concrete type.
                if (!registrationQuery.Any())
                {
                    if (ConcreteType == null)
                        registrationQuery = this._typeRegistrations.Where(
                            c => c.GetRegisteredType() == TypeToResolve);
                    else
                        registrationQuery = this._typeRegistrations.Where(
                            c => c.GetRegisteredType() == TypeToResolve && c.GetRegisteredType() == ConcreteType);
                }
            }

            if (!registrationQuery.Any())
                return null;
            else if (registrationQuery.Count() > 1)
            {
                IDependencyRegistration registration = null;

                foreach (var item in registrationQuery)
                {
                    if (item.GetRegisteredTypeAsDefault())
                    {
                        registration = item;
                        break;
                    }
                }

                if (registration == null)
                    registration = registrationQuery.First();

                return registration;
            }
            else
                return registrationQuery.First();
        }
    }
}
