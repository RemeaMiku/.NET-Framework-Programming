using System;

namespace AtmSimulator.Exceptions;

class BadCashException : ApplicationException
{
    #region Public Constructors

    public BadCashException() { }
    public BadCashException(string message) : base(message) { }

    #endregion Public Constructors
}
