using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IModuleFactory
    {
        // configuration.
        IModuleFactory AsEmitSymbolInfo();
        IModuleFactory ToFileName(string FileName);

        // service.
        void Generate();
        ITypeFactory CreateType(string TypeName);
    }
}
