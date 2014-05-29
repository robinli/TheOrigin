using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class TypeIncompatibilityException : Exception
    {
        public TypeIncompatibilityException(string Message) : base(Message)
        {
        }
    }
}
