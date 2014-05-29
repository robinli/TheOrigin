using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class TypeRegistrationException : Exception
    {
        public TypeRegistrationException(string Message) : base(Message)
        {
        }
    }
}
