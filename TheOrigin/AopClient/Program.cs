using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOrigin.Framework.AOP;
using TheOrigin.Framework.DependencyInjection;

namespace AopClient
{
    class Program
    {
        public class Program2
        {
            public static void Main(string[] args)
            {
                //var reg = ProxyServiceConfiguration
                //    .Define<IMyObject2>()
                //    .Implemented<MyObject2>()
                //    .RegisterInterceptor(new LogInterceptor());                

                //var o = reg.CreateProxy<IMyObject2>();

                // interceptor registrations.
                InterceptorRegistrationService.For<IMyObject2>()
                    .PropertyIntercept("MyProperty", new LogPropertyInterceptor())
                    .MethodIntercept("Method1", new LogInterceptor())
                    .MethodIntercept("Method1", new LogInterceptor2())
                    .MethodIntercept("Method2", new LogInterceptor());

                var proxyService = AOPServiceProvider.Resolve<IDynamicProxyService>(typeof(IMyObject2));
                var o = proxyService.CreateProxy<IMyObject2>(new MyObject2());

                o.MyProperty = 123;
                int a = o.MyProperty;
                o.MyProperty = 456;
                o.Method1();
                o.Method2(1, 2);

                Console.Write("OK");
                Console.ReadLine();
            }
        }
    }


    public class LogInterceptor : IInterceptor
    {
        public void BeforeExecute(string Method, Dictionary<string, object> InvokeArgs)
        {
            Console.WriteLine("BeforeExecute");
            Console.WriteLine("arg count: {0}", InvokeArgs.Count);
            Console.WriteLine("End BeforeExecute");
        }

        public void AfterExecute(string Method, Dictionary<string, object> InvokeArgs, object ReturnValue)
        {
            Console.WriteLine("AfterExecute");
            Console.WriteLine("arg count: {0}", InvokeArgs.Count);
            Console.WriteLine("return: {0}", ReturnValue);
            Console.WriteLine("End AfterExecute");
        }
    }

    public class LogInterceptor2 : IInterceptor
    {
        public void BeforeExecute(string Method, Dictionary<string, object> InvokeArgs)
        {
            Console.WriteLine("BeforeExecute2");
            Console.WriteLine("arg count: {0}", InvokeArgs.Count);
            Console.WriteLine("End BeforeExecute2");
        }

        public void AfterExecute(string Method, Dictionary<string, object> InvokeArgs, object ReturnValue)
        {
            Console.WriteLine("AfterExecute2");
            Console.WriteLine("arg count: {0}", InvokeArgs.Count);
            Console.WriteLine("return: {0}", ReturnValue);
            Console.WriteLine("End AfterExecute2");
        }
    }

    public class LogPropertyInterceptor : IPropertyInterceptor
    {
        public bool SupportGet { get { return true; } }
        public bool SupportSet { get { return true; } }

        public void Intercept(string PropertyName, PropertyOpType OpType, object PropertyValue)
        {
            if ((SupportGet && OpType == PropertyOpType.Get) || (SupportSet && OpType == PropertyOpType.Set))
            {
                Console.WriteLine("Property: BeforeExecute");
                Console.WriteLine("OpType : {0}, Value: {1}", Enum.GetName(typeof(PropertyOpType), OpType), PropertyValue ?? "NULL");
                Console.WriteLine("Property: End BeforeExecute");
            }
        }
    }

    public class MyObject2 : IMyObject2
    {
        public int MyProperty { get; set; }

        public void Method1()
        {
            Console.WriteLine("Invoking Method 1");
        }

        public int Method2(int a, int b)
        {
            Console.WriteLine("Invoking Method 2");
            return a + b;
        }
    }

    public interface IMyObject2
    {
        int MyProperty { get; set; }
        void Method1();
        int Method2(int a, int b);
    }
}
