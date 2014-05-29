using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOrigin.Framework.AOP.MethodBodyImplFactories;
using TheOrigin.Framework.DependencyInjection;

namespace TheOrigin.Framework.AOP
{
    public class AOPServiceProvider
    {
        private static IContainerBuilder aopContainerBuilder = new ContainerBuilder();
        private static IContainer aopContainer = null;

        static AOPServiceProvider()
        {
            aopContainerBuilder.Register<DefaultAssemblyFactory>().AsInterface<IAssemblyFactory>();
            aopContainerBuilder.Register<DefaultModuleFactory>().AsInterface<IModuleFactory>();
            aopContainerBuilder.Register<DefaultTypeFactory>().AsInterface<ITypeFactory>();
            aopContainerBuilder.Register<DefaultPropertyFactory>().AsInterface<IPropertyFactory>();
            aopContainerBuilder.Register<DefaultMethodFactory>().AsInterface<IMethodFactory>();
            aopContainerBuilder.Register<DefaultConstructorFactory>().AsInterface<IConstructorFactory>();
            aopContainerBuilder.Register<PropertyAutoImplGetMethodBodyFactory>().AsInterface<IMethodBodyFactory>();
            aopContainerBuilder.Register<PropertyAutoImplSetMethodBodyFactory>().AsInterface<IMethodBodyFactory>();
            aopContainerBuilder.Register<DefaultConstructorMethodBodyFactory>().AsInterface<IMethodBodyFactory>();
            aopContainerBuilder.Register<DynamicProxyService>().AsInterface<IDynamicProxyService>();
            aopContainerBuilder.Register<SimpleDynamicProxyService>().AsInterface<ISimpleDynamicProxyService>();
        }

        private static void EnsureContainerInited()
        {
            if (aopContainer == null)
                aopContainer = aopContainerBuilder.GetContainer();
        }

        public static TInterface Resolve<TInterface>()
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public static TInterface Resolve<TInterface>(params object[] args)
        {
            return (TInterface)Resolve(typeof(TInterface), args);
        }

        public static TInterface Resolve<TInterface, TConcreteClass>()
        {
            return (TInterface)ResolveForConcreteType(typeof(TInterface), typeof(TConcreteClass));
        }

        public static TInterface Resolve<TInterface, TConcreteClass>(params object[] args)
        {
            return (TInterface)ResolveForConcreteType(typeof(TInterface), typeof(TConcreteClass), args);
        }

        public static object Resolve(Type TypeToResolve, params object[] args)
        {
            EnsureContainerInited();

            return aopContainer.Resolve(TypeToResolve, args);
        }

        public static object ResolveForConcreteType(Type TypeToResolve, Type TypeConcreteClass, params object[] args)
        {
            EnsureContainerInited();

            return aopContainer.Resolve(TypeToResolve, TypeConcreteClass, args);
        }
    }
}
