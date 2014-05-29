using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class SimpleDynamicProxyService : ISimpleDynamicProxyService
    {
        private Type _proxyType = null;
        private Type _proxyTargetType = null;
        private Assembly _proxyTypeContainer = null;
        private IAssemblyFactory _assemblyFactory = null;
        private IModuleFactory _modFactory = null;
        private ITypeFactory _proxyTypeFactory = null;
        private FieldInfo _interceptorField = null;
        private FieldInfo _interceptorPropertyField = null;
        private FieldInfo _instanceField = null;
        private List<string> _propertyMethodNames = new List<string>();

        public SimpleDynamicProxyService(Type TypeToProxy)
        {
            Type baseType = typeof(SimpleDynamicProxyBase);

            this._proxyType = TypeToProxy;
            this._interceptorField = baseType.GetField("_interceptor", BindingFlags.Instance | BindingFlags.Public);
            this._interceptorPropertyField = baseType.GetField("_propertyInterceptor", BindingFlags.Instance | BindingFlags.Public);
            this._instanceField = baseType.GetField("_proxyInstance", BindingFlags.Instance | BindingFlags.Public);
        }

        public SimpleDynamicProxyService(Type TypeToProxy, Assembly ProxyTypeContainer) : this(TypeToProxy)
        {
            this._proxyTypeContainer = ProxyTypeContainer;
        }

        public T CreateProxy<T>(IInterceptor Interceptor, IPropertyInterceptor PropertyInterceptor, object ProxyInstance)
        {
            return (T)this.CreateProxy(Interceptor, PropertyInterceptor, ProxyInstance);
        }

        public object CreateProxy(IInterceptor Interceptor, IPropertyInterceptor PropertyInterceptor, object ProxyInstance)
        {
            if (this._proxyTargetType == null)
            {
                this._assemblyFactory = AOPServiceProvider.Resolve<IAssemblyFactory>().Define("TheOriginFrameworkAOPService");
                this._modFactory = this._assemblyFactory.CreateModule("SimpleProxyServiceModule");
                this._proxyTypeFactory = this._modFactory.CreateType("SimpleProxyService$" + DateTime.Now.Ticks.ToString())
                    .AsPublicClass()
                    .FromParent(typeof(SimpleDynamicProxyBase))
                    .ImplementInterface(this._proxyType);

                var defConstructor = this._proxyTypeFactory.CreateConstructor()
                        .WithParameter<IInterceptor>()
                        .WithParameter<IPropertyInterceptor>()
                        .WithParameter<object>()
                        .ImplementBody(x =>
                        {
                            // this._interceptorField = p1;
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_1);
                            x.Emit(OpCodes.Stfld, this._interceptorField);
                            // this._propertyInterceptor = p2;
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_2);
                            x.Emit(OpCodes.Stfld, this._interceptorPropertyField);
                            // this._instanceField = p3;
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_3);
                            x.Emit(OpCodes.Stfld, this._instanceField);
                            x.Emit(OpCodes.Ret);
                        });

                // create property and methods.
                var properties = this._proxyType.GetProperties();
                var methods = this._proxyType.GetMethods();

                foreach (var property in properties)
                {
                    this.CreateProperty(property.Name, property.PropertyType);
                }

                foreach (var method in methods)
                {
                    this.CreateMethod(method);
                }

                // complete, return result.
                this._proxyTargetType = this._proxyTypeFactory.GetTypeFromFactory();
            }

            var o = this._proxyTargetType
                .GetConstructor(new Type[] { typeof(IInterceptor), typeof(IPropertyInterceptor), typeof(object) })
                .Invoke(new object[] { Interceptor, PropertyInterceptor, ProxyInstance });

            return o;
        }

        private void CreateProperty(string PropertyName, Type PropertyType)
        {
            this._proxyTypeFactory.CreateProperty(PropertyName, PropertyType)
                .ForImplementFromInterface()
                .ImplementGetBody(x =>
                {
                    MethodInfo executeGetMethod = typeof(IPropertyInterceptor).GetMethod("Intercept");

                    if (executeGetMethod != null)
                    {
                        x.Emit(OpCodes.Ldarg_0);
                        x.Emit(OpCodes.Ldfld, this._interceptorPropertyField);
                        x.Emit(OpCodes.Ldstr, PropertyName);
                        x.Emit(OpCodes.Ldc_I4, (int)PropertyOpType.Get); 
                        x.Emit(OpCodes.Ldloc_0);

                        if (PropertyType.IsPrimitive || PropertyType.IsValueType)
                            x.Emit(OpCodes.Box, PropertyType); // value type must box before call.

                        x.Emit(OpCodes.Call, executeGetMethod);
                    }
                })
                .ImplementSetBody(x =>
                {
                    MethodInfo executeSetMethod = typeof(IPropertyInterceptor).GetMethod("Intercept");

                    if (executeSetMethod != null)
                    {
                        x.Emit(OpCodes.Ldarg_0);
                        x.Emit(OpCodes.Ldfld, this._interceptorPropertyField);
                        x.Emit(OpCodes.Ldstr, PropertyName);
                        x.Emit(OpCodes.Ldc_I4, (int)PropertyOpType.Set);
                        x.Emit(OpCodes.Ldloc_0);

                        if (PropertyType.IsPrimitive || PropertyType.IsValueType)
                            x.Emit(OpCodes.Box, PropertyType); // value type must box before call.

                        x.Emit(OpCodes.Call, executeSetMethod);
                    }
                });

            this._propertyMethodNames.Add("get_" + PropertyName);
            this._propertyMethodNames.Add("set_" + PropertyName);
        }

        private void CreateMethod(MethodInfo Method)
        {
            if (this._propertyMethodNames.Contains(Method.Name))
                return;

            var methodFactory = this._proxyTypeFactory.CreateMethod(Method.Name).ForImplementFromInterface();

            if (Method.ReturnType != typeof(void))
                methodFactory.WithReturn(Method.ReturnType);

            var parameters = Method.GetParameters();

            foreach (var param in parameters)
                methodFactory.WithParameter(param.ParameterType);

            methodFactory.ImplementBody((x, ps) =>
            {
                LocalBuilder retValBuilder = (Method.ReturnType != typeof(void)) ? x.DeclareLocal(Method.ReturnType) : null;
                LocalBuilder paramContainerBuilder = x.DeclareLocal(typeof(Dictionary<string, object>));
                ConstructorInfo paramContainerConstructorInfo = typeof(Dictionary<string, object>).GetConstructor(new Type[0]);
                MethodInfo beforeExecuteMethod = typeof(IInterceptor).GetMethod("BeforeExecute");
                MethodInfo afterExecuteMethod = typeof(IInterceptor).GetMethod("AfterExecute");

                // create parameter info container.
                x.Emit(OpCodes.Newobj, paramContainerConstructorInfo);
                x.Emit(OpCodes.Stloc, paramContainerBuilder);

                int paramIdx = 1;

                foreach (var param in Method.GetParameters())
                {
                    // place parameter information.
                    x.Emit(OpCodes.Ldloc, paramContainerBuilder);
                    x.Emit(OpCodes.Ldstr, param.Name);

                    if (paramIdx < 4)
                    {
                        switch (paramIdx)
                        {
                            case 1:
                                x.Emit(OpCodes.Ldarg_1);
                                break;
                            case 2:
                                x.Emit(OpCodes.Ldarg_2);
                                break;
                            case 3:
                                x.Emit(OpCodes.Ldarg_3);
                                break;
                        }
                    }
                    else
                        x.Emit(OpCodes.Ldarg_S, paramIdx);

                    // box value type.
                    if (param.ParameterType.IsPrimitive || param.ParameterType.IsValueType)
                        x.Emit(OpCodes.Box, param.ParameterType);

                    // call.
                    x.Emit(OpCodes.Callvirt, typeof(Dictionary<string, object>).GetMethod("Add"));

                    paramIdx++;
                }

                // invoke pre-call method in interceptor.
                if (beforeExecuteMethod != null)
                {
                    x.Emit(OpCodes.Ldarg_0);
                    x.Emit(OpCodes.Ldfld, this._interceptorField);
                    x.Emit(OpCodes.Ldstr, Method.Name);
                    x.Emit(OpCodes.Ldloc, paramContainerBuilder);
                    x.Emit(OpCodes.Callvirt, beforeExecuteMethod);
                }

                // invoke main method.
                x.Emit(OpCodes.Ldarg_0);
                x.Emit(OpCodes.Ldfld, this._instanceField);

                paramIdx = 1;

                foreach (var param in Method.GetParameters())
                {
                    if (paramIdx < 4)
                    {
                        switch (paramIdx)
                        {
                            case 1:
                                x.Emit(OpCodes.Ldarg_1);
                                break;
                            case 2:
                                x.Emit(OpCodes.Ldarg_2);
                                break;
                            case 3:
                                x.Emit(OpCodes.Ldarg_3);
                                break;
                        }
                    }
                    else
                        x.Emit(OpCodes.Ldarg_S, paramIdx);

                    paramIdx++;
                }

                x.Emit(OpCodes.Call, Method);

                // place return value if needed.
                if (retValBuilder != null)
                    x.Emit(OpCodes.Stloc, retValBuilder);

                if (afterExecuteMethod != null)
                {
                    // invoke post-call method in interceptor.
                    x.Emit(OpCodes.Ldarg_0);
                    x.Emit(OpCodes.Ldfld, this._interceptorField);
                    x.Emit(OpCodes.Ldstr, Method.Name);
                    x.Emit(OpCodes.Ldloc, paramContainerBuilder);

                    if (retValBuilder != null)
                    {
                        x.Emit(OpCodes.Ldloc, retValBuilder);
                        x.Emit(OpCodes.Box, Method.ReturnType);
                    }
                    else
                        x.Emit(OpCodes.Ldnull);

                    x.Emit(OpCodes.Callvirt, afterExecuteMethod);
                }

                // place return value if needed.
                if (retValBuilder != null)
                    x.Emit(OpCodes.Ldloc, retValBuilder);

                // complete, return.
                x.Emit(OpCodes.Nop);
                x.Emit(OpCodes.Ret);
            });
        }
    }
}
