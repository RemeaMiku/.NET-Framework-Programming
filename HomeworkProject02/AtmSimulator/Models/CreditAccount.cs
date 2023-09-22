using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtmSimulator.Models;

public class CreditAccount : Account
{
    #region Public Constructors

    public CreditAccount(int accountNumber, string idNumber, string nameOfHolder, string password, Bank bank, decimal creditLimit) : base(accountNumber, idNumber, nameOfHolder, password, bank)
    {
        if (creditLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(creditLimit));
        CreditLimit = creditLimit;
        Credit = creditLimit;
    }

    #endregion Public Constructors

    #region Public Properties

    public decimal CreditLimit { get; init; }
    public decimal Credit { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public override void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (Credit < CreditLimit)
        {
            var requiredCredit = CreditLimit - Credit;
            if (amount > requiredCredit)
            {
                Credit = CreditLimit;
                Balance += amount - requiredCredit;
            }
            else
                Credit += amount;
        }
        else
            base.Deposit(amount);
    }

    public override void WithDrawal(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > Balance)
        {
            var requiredCredit = amount - Balance;
            if (requiredCredit > Credit)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Credit -= requiredCredit;
            Balance = 0;
            Log(false, amount);
        }
        else
            base.WithDrawal(amount);
    }

    #endregion Public Methods
}
