using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultAssemblyFactory : IAssemblyFactory
    {
        private string _assemblyName = null, _fileName = null;
        private bool _appendRandomIdentifier = false;
        private AssemblyBuilderAccess _builderAccess = AssemblyBuilderAccess.Run;
        private AppDomain _domain = null;
        private AssemblyBuilder _builder = null;

        public DefaultAssemblyFactory()
        {
            this._domain = AppDomain.CurrentDomain;
        }

        public DefaultAssemblyFactory(AppDomain Domain)
        {
            this._domain = Domain;
        }

        public IAssemblyFactory Define(string AssemblyName)
        {
            this._assemblyName = AssemblyName;
            return this;
        }

        public IAssemblyFactory Define(string AssemblyName, bool AppendRandomIdentifier)
        {
            this._assemblyName = AssemblyName;
            this._appendRandomIdentifier = AppendRandomIdentifier;
            return this;
        }

        public IAssemblyFactory ForAccess(AssemblyBuilderAccess AccessFlags)
        {
            this._builderAccess = AccessFlags;
            return this;
        }

        public IAssemblyFactory ToFileName(string FileName)
        {
            this._fileName = FileName;
            return this;
        }

        public IModuleFactory CreateModule(string ModuleName)
        {
            if (this._builder == null)
                this.Generate();

            return AOPServiceProvider.Resolve<IModuleFactory>(this._builder, ModuleName);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(this._fileName))
                this._builder.Save(this._domain.BaseDirectory + string.Format(@"\{0}.dll", this._assemblyName));
            else
                this._builder.Save(this._fileName);
        }

        public void Generate()
        {
            if (!string.IsNullOrEmpty(this._fileName))
                this._builder = this._domain.DefineDynamicAssembly(
                    new AssemblyName(this._assemblyName),
                    this._builderAccess, Path.GetDirectoryName(this._fileName));
            else
                this._builder = this._domain.DefineDynamicAssembly(
                    new AssemblyName(this._assemblyName), this._builderAccess);
        }
    }
}
