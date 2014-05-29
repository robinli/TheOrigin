using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TheOrigin.Framework.AOP
{
    public interface IAssemblyFactory
    {
        // configuration.
        IAssemblyFactory Define(string AssemblyName);
        IAssemblyFactory Define(string AssemblyName, bool AppendRandomIdentifier);
        IAssemblyFactory ForAccess(AssemblyBuilderAccess AccessFlags);
        IAssemblyFactory ToFileName(string FileName);

        // service.
        void Generate();
        void Save();
        IModuleFactory CreateModule(string ModuleName);
    }
}
