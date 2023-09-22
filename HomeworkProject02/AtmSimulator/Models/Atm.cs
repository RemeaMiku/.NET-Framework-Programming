using System.Collections.Generic;

namespace AtmSimulator.Models;

public class Atm
{
    #region Public Properties

    public Account? CurrentAccount { get; set; }
    public List<Bank> Banks { get; } = new();

    #endregion Public Properties
}
