using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class DefaultModuleFactory : IModuleFactory
    {
        private string _moduleName = null;
        private bool _emitSymbolInfo = false;
        private string _fileName = null;
        private AssemblyBuilder _assemblyBuilder = null;
        private ModuleBuilder _moduleBuilder = null;

        public DefaultModuleFactory(AssemblyBuilder Builder, string ModuleName)
        {
            this._assemblyBuilder = Builder;
            this._moduleName = ModuleName;
        }

        public IModuleFactory AsEmitSymbolInfo()
        {
            this._emitSymbolInfo = true;
            return this;
        }

        public IModuleFactory ToFileName(string FileName)
        {
            this._fileName = FileName;
            return this;
        }

        public ITypeFactory CreateType(string TypeName)
        {
            if (this._moduleBuilder == null)
                this.Generate();

            return AOPServiceProvider.Resolve<ITypeFactory>(this._moduleBuilder, TypeName);
        }

        public void Generate()
        {
            if (!string.IsNullOrEmpty(this._fileName))
                this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(this._moduleName, this._fileName, this._emitSymbolInfo);
            else
                this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(this._moduleName, this._emitSymbolInfo);
        }
    }
}
