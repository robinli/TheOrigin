using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IPropertyInterceptor
    {
        bool SupportGet { get; }
        bool SupportSet { get; }

        void Intercept(string PropertyName, PropertyOpType OpType, object PropertyValue);
    }
}
