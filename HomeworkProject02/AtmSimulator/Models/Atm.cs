using System.Collections.Generic;
using System.Windows.Documents;

namespace AtmSimulator.Models;

public class Atm
{
    #region Public Properties

    public Account? CurrentAccount { get; set; }
    public List<Bank> Banks { get; } = new();

    #endregion Public Properties
}
