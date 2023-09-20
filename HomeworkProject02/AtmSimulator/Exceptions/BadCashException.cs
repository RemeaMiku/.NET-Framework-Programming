using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmSimulator.Exceptions;

class BadCashException : ApplicationException
{
    public BadCashException() { }
    public BadCashException(string message) : base(message) { }
}
