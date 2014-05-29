using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DynamicProxyService : IDynamicProxyService
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
        private FieldInfo _typeField = null;
        private MethodInfo _getPropertyInterceptorMethod = null;
        private MethodInfo _getMethodInterceptorMethod = null;
        private List<string> _propertyMethodNames = new List<string>();

        public DynamicProxyService(Type TypeToProxy)
        {
            Type baseType = typeof(DynamicProxyBase);

            this._proxyType = TypeToProxy;
            this._interceptorField = baseType.GetField("MethodInterceptorRegistrations", BindingFlags.Instance | BindingFlags.Public);
            this._interceptorPropertyField = baseType.GetField("PropertyInterceptorRegistrations", BindingFlags.Instance | BindingFlags.Public);
            this._instanceField = baseType.GetField("ConcreteObjectProxy", BindingFlags.Instance | BindingFlags.Public);
            this._typeField = baseType.GetField("ProxyType", BindingFlags.Instance | BindingFlags.Public);
            this._getMethodInterceptorMethod = baseType.GetMethod("GetMethodInterceptors", BindingFlags.Instance | BindingFlags.NonPublic);
            this._getPropertyInterceptorMethod = baseType.GetMethod("GetPropertyInterceptors", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public DynamicProxyService(Type TypeToProxy, Assembly ProxyTypeContainer)
            : this(TypeToProxy)
        {
            this._proxyTypeContainer = ProxyTypeContainer;
        }

        public T CreateProxy<T>(object ProxyInstance)
        {
            return (T)this.CreateProxy(ProxyInstance);
        }

        public object CreateProxy(object ProxyInstance)
        {
            if (this._proxyTargetType == null)
            {
                this._assemblyFactory = AOPServiceProvider.Resolve<IAssemblyFactory>().Define("TheOriginFrameworkAOPService");
                this._modFactory = this._assemblyFactory.CreateModule("ProxyServiceModule");
                this._proxyTypeFactory = this._modFactory.CreateType("ProxyService$" + DateTime.Now.Ticks.ToString())
                    .AsPublicClass()
                    .FromParent(typeof(DynamicProxyBase))
                    .ImplementInterface(this._proxyType);

                var defConstructor = this._proxyTypeFactory.CreateConstructor()
                        .WithParameter<Type>()
                        .WithParameter<object>()
                        .ImplementBody(x =>
                        {
                            // call base constructor
                            var baseCtor = typeof(DynamicProxyBase).GetConstructor(new Type[] { typeof(Type), typeof(object) });

                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_1);
                            x.Emit(OpCodes.Ldarg_2);
                            x.Emit(OpCodes.Call, baseCtor);

                            // this.ConcreteObjectProxy = p0;
                            x.Emit(OpCodes.Ldarg_0);
                            x.Emit(OpCodes.Ldarg_2);
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
                .GetConstructor(new Type[] { typeof(Type), typeof(object) })
                .Invoke(new object[] { this._proxyType, ProxyInstance });

            return o;
        }

        private void CreateProperty(string PropertyName, Type PropertyType)
        {
            MethodInfo propertyInterceptMethod = typeof(IPropertyInterceptor).GetMethod("Intercept");
            PropertyInfo supportGetProperty = typeof(IPropertyInterceptor).GetProperty("SupportGet");
            PropertyInfo supportSetProperty = typeof(IPropertyInterceptor).GetProperty("SupportSet");

            this._proxyTypeFactory.CreateProperty(PropertyName, PropertyType)
                .ForImplementFromInterface()
                .ImplementGetBody(x =>
                {
                    // the following Emit call is equalivant to "var prop = typeof(Type).GetProperty('Name');
                    LocalBuilder propertyVar = x.DeclareLocal(typeof(PropertyInfo));
                    x.Emit(OpCodes.Ldtoken, this._proxyType);
                    x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    x.Emit(OpCodes.Ldstr, PropertyName);
                    x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string) }));
                    x.Emit(OpCodes.Stloc, propertyVar);

                    // the following Emit call is equalivant to "var list = this.GetPropertyInterceptors(prop);"
                    LocalBuilder interceptorVar = x.DeclareLocal(typeof(IList<IPropertyInterceptor>));
                    x.Emit(OpCodes.Ldarg_0);
                    x.Emit(OpCodes.Ldloc, propertyVar);
                    x.Emit(OpCodes.Call, this._getPropertyInterceptorMethod);
                    x.Emit(OpCodes.Stloc, interceptorVar);

                    // the following Emit call is equalivant to "var iterator = list.GetEnumerator();"
                    LocalBuilder iteratorVar = x.DeclareLocal(typeof(IEnumerator<IPropertyInterceptor>));
                    x.Emit(OpCodes.Ldloc, interceptorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerable<IPropertyInterceptor>).GetMethod("GetEnumerator"));
                    x.Emit(OpCodes.Stloc, iteratorVar);

                    // the following Emit call will navigate elements in collection (like foreach)
                    Label whileLabel = x.DefineLabel();
                    Label trueLabel = x.DefineLabel();
                    LocalBuilder loopStateVar = x.DeclareLocal(typeof(bool));

                    x.Emit(OpCodes.Br_S, whileLabel);

                    // define TRUE block.
                    x.MarkLabel(trueLabel);

                    // call iterator.Current to get element.
                    x.Emit(OpCodes.Nop);
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IPropertyInterceptor>).GetProperty("Current").GetGetMethod());

                    // check SupportGet=true.
                    // equalivant "if (iterator.Current.SupportGet)"
                    LocalBuilder supportStateVar = x.DeclareLocal(typeof(bool));
                    x.Emit(OpCodes.Callvirt, supportGetProperty.GetGetMethod());
                    x.Emit(OpCodes.Ldc_I4_0);
                    x.Emit(OpCodes.Ceq);
                    x.Emit(OpCodes.Stloc_S, supportStateVar);
                    x.Emit(OpCodes.Ldloc_S, supportStateVar);
                    x.Emit(OpCodes.Brtrue_S, whileLabel);

                    // call element's method.
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IPropertyInterceptor>).GetProperty("Current").GetGetMethod());
                    x.Emit(OpCodes.Ldstr, PropertyName);
                    x.Emit(OpCodes.Ldc_I4_0);
                    x.Emit(OpCodes.Ldloc_0); // field value in LocalBuilder defined in DefaultPropertyFactory.Generate().

                    if (PropertyType.IsPrimitive || PropertyType.IsValueType)
                        x.Emit(OpCodes.Box, PropertyType); // value type must box before call.

                    x.Emit(OpCodes.Callvirt, propertyInterceptMethod);

                    x.Emit(OpCodes.Nop);
                    x.Emit(OpCodes.Nop);

                    // define CONDITION block.
                    x.MarkLabel(whileLabel);

                    // while check.
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
                    x.Emit(OpCodes.Stloc, loopStateVar);
                    x.Emit(OpCodes.Ldloc, loopStateVar);
                    x.Emit(OpCodes.Brtrue_S, trueLabel);
                })
                .ImplementSetBody(x =>
                {
                    // the following Emit call is equalivant to "var prop = typeof(Type).GetProperty('Name');
                    LocalBuilder propertyVar = x.DeclareLocal(typeof(PropertyInfo));
                    x.Emit(OpCodes.Ldtoken, this._proxyType);
                    x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    x.Emit(OpCodes.Ldstr, PropertyName);
                    x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string) }));
                    x.Emit(OpCodes.Stloc, propertyVar);

                    // the following Emit call is equalivant to "var list = this.GetPropertyInterceptors(prop);"
                    LocalBuilder interceptorVar = x.DeclareLocal(typeof(IList<IPropertyInterceptor>));
                    x.Emit(OpCodes.Ldarg_0);
                    x.Emit(OpCodes.Ldloc, propertyVar);
                    x.Emit(OpCodes.Call, this._getPropertyInterceptorMethod);
                    x.Emit(OpCodes.Stloc, interceptorVar);

                    // the following Emit call is equalivant to "var iterator = list.GetEnumerator();"
                    LocalBuilder iteratorVar = x.DeclareLocal(typeof(IEnumerator<IPropertyInterceptor>));
                    x.Emit(OpCodes.Ldloc, interceptorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerable<IPropertyInterceptor>).GetMethod("GetEnumerator"));
                    x.Emit(OpCodes.Stloc, iteratorVar);

                    // the following Emit call will navigate elements in collection (like foreach)
                    Label whileLabel = x.DefineLabel();
                    Label trueLabel = x.DefineLabel();
                    LocalBuilder loopStateVar = x.DeclareLocal(typeof(bool));

                    x.Emit(OpCodes.Br_S, whileLabel);

                    // define TRUE block.
                    x.MarkLabel(trueLabel);

                    // call iterator.Current to get element.
                    x.Emit(OpCodes.Nop);
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IPropertyInterceptor>).GetProperty("Current").GetGetMethod());

                    // check SupportSet=true.
                    // equalivant "if (iterator.Current.SupportGet)"
                    LocalBuilder supportStateVar = x.DeclareLocal(typeof(bool));
                    x.Emit(OpCodes.Callvirt, supportSetProperty.GetGetMethod());
                    x.Emit(OpCodes.Ldc_I4_0);
                    x.Emit(OpCodes.Ceq);
                    x.Emit(OpCodes.Stloc_S, supportStateVar);
                    x.Emit(OpCodes.Ldloc_S, supportStateVar);
                    x.Emit(OpCodes.Brtrue_S, whileLabel);

                    // call element's method.
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IPropertyInterceptor>).GetProperty("Current").GetGetMethod());
                    x.Emit(OpCodes.Ldstr, PropertyName);
                    x.Emit(OpCodes.Ldc_I4_1);
                    x.Emit(OpCodes.Ldarg_1); // "value" in set property.

                    if (PropertyType.IsPrimitive || PropertyType.IsValueType)
                        x.Emit(OpCodes.Box, PropertyType); // value type must box before call.

                    x.Emit(OpCodes.Callvirt, propertyInterceptMethod);

                    x.Emit(OpCodes.Nop);
                    x.Emit(OpCodes.Nop);

                    // define CONDITION block.
                    x.MarkLabel(whileLabel);

                    // while check.
                    x.Emit(OpCodes.Ldloc, iteratorVar);
                    x.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
                    x.Emit(OpCodes.Stloc, loopStateVar);
                    x.Emit(OpCodes.Ldloc, loopStateVar);
                    x.Emit(OpCodes.Brtrue_S, trueLabel);
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


                // the following Emit call is equalivant to "var method = typeof(Type).GetMethod('Name');
                LocalBuilder methodVar = x.DeclareLocal(typeof(MethodInfo));
                x.Emit(OpCodes.Ldtoken, this._proxyType);
                x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                x.Emit(OpCodes.Ldstr, Method.Name);
                x.Emit(OpCodes.Call, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) }));
                x.Emit(OpCodes.Stloc, methodVar);

                // the following Emit call is equalivant to "var list = this.GetMethodInterceptors(method);"
                LocalBuilder interceptorVar = x.DeclareLocal(typeof(IList<IInterceptor>));
                x.Emit(OpCodes.Ldarg_0);
                x.Emit(OpCodes.Ldloc, methodVar);
                x.Emit(OpCodes.Call, this._getMethodInterceptorMethod);
                x.Emit(OpCodes.Stloc, interceptorVar);

                // the following Emit call is equalivant to "var iterator = list.GetEnumerator();"
                LocalBuilder iteratorVar = x.DeclareLocal(typeof(IEnumerator<IInterceptor>));
                x.Emit(OpCodes.Ldloc, interceptorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerable<IInterceptor>).GetMethod("GetEnumerator"));
                x.Emit(OpCodes.Stloc, iteratorVar);

                // the following Emit call will navigate elements in collection (like foreach)
                Label whileLabel = x.DefineLabel();
                Label trueLabel = x.DefineLabel();
                LocalBuilder loopStateVar = x.DeclareLocal(typeof(bool));

                x.Emit(OpCodes.Br_S, whileLabel);

                // define TRUE block.
                x.MarkLabel(trueLabel);

                // call iterator.Current to get element.
                x.Emit(OpCodes.Nop);
                x.Emit(OpCodes.Ldloc, iteratorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IInterceptor>).GetProperty("Current").GetGetMethod());

                // call element's method (BeforeExecute)
                x.Emit(OpCodes.Ldstr, Method.Name);
                x.Emit(OpCodes.Ldloc, paramContainerBuilder);
                x.Emit(OpCodes.Callvirt, beforeExecuteMethod);

                x.Emit(OpCodes.Nop);
                x.Emit(OpCodes.Nop);

                // define CONDITION block.
                x.MarkLabel(whileLabel);

                // while check.
                x.Emit(OpCodes.Ldloc, iteratorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
                x.Emit(OpCodes.Stloc, loopStateVar);
                x.Emit(OpCodes.Ldloc, loopStateVar);
                x.Emit(OpCodes.Brtrue_S, trueLabel);
                                                
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
                
                // get enumerator again and execute AfterExecute()
                x.Emit(OpCodes.Ldloc, interceptorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerable<IInterceptor>).GetMethod("GetEnumerator"));
                x.Emit(OpCodes.Stloc, iteratorVar);

                // the following Emit call will navigate elements in collection (like foreach)
                Label whileLabel2 = x.DefineLabel();
                Label trueLabel2 = x.DefineLabel();
                LocalBuilder loopStateVar2 = x.DeclareLocal(typeof(bool));

                x.Emit(OpCodes.Br_S, whileLabel2);

                // define TRUE block.
                x.MarkLabel(trueLabel2);

                // call iterator.Current to get element.
                x.Emit(OpCodes.Nop);
                x.Emit(OpCodes.Ldloc, iteratorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerator<IInterceptor>).GetProperty("Current").GetGetMethod());

                // call element's method (AfterExecute)
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

                x.Emit(OpCodes.Nop);
                x.Emit(OpCodes.Nop);

                // define CONDITION block.
                x.MarkLabel(whileLabel2);

                // while check.
                x.Emit(OpCodes.Ldloc, iteratorVar);
                x.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
                x.Emit(OpCodes.Stloc, loopStateVar2);
                x.Emit(OpCodes.Ldloc, loopStateVar2);
                x.Emit(OpCodes.Brtrue_S, trueLabel2);
                
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
