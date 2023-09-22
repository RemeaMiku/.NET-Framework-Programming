using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmSimulator.Exceptions;

class BadCashException : ApplicationException
{
    #region Public Constructors

    public BadCashException() { }
    public BadCashException(string message) : base(message) { }

    #endregion Public Constructors
}
