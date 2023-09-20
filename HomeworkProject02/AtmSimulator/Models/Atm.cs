using System.Collections.Generic;
using System.Windows.Documents;

namespace AtmSimulator.Models;

public class Atm
{
    public Account? CurrentAccount { get; set; }
    public List<Bank> Banks { get; } = new();
}
