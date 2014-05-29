using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.DependencyInjection
{
    public class RegistrationNotFoundException : Exception
    {
        public RegistrationNotFoundException(string Message) : base(Message)
        {
        }
    }
}
