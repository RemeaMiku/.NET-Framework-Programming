using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtmSimulator.Exceptions;
using AtmSimulator.Models;

namespace AtmSimulator.Services;

public class AtmService
{
    #region Public Fields

    public const decimal BigMoneyLimit = 10000M;

    #endregion Public Fields

    #region Public Constructors

    public AtmService(Atm atm)
    {
        _atm = atm;
    }

    #endregion Public Constructors

    #region Public Events

    public event EventHandler<BigMoneyEventArgs>? BigMoneyFetched;

    #endregion Public Events

    #region Public Methods

    public IEnumerable<string> GetBankNames()
    {
        foreach (var bank in _atm.Banks)
            yield return bank.Name;
    }

    public async Task<Account?> LoginAsync(string bankName, string phoneNumber, string password)
    {
        var account = default(Account);
        Account.ValidatePhoneNumber(phoneNumber);
        Account.ValidatePassword(password);
        var bank = _atm.Banks.Find(b => b.Name == bankName) ??
            throw new ArgumentException("This ATM does not support this bank or this bank does not exist.");
        await Task.Delay(1500);
        account = bank[phoneNumber] ??
            throw new ArgumentException("The account registered with this phone number does not exist.");
        if (account.Password != password)
            throw new ArgumentException("Password Error.");
        _account = account;
        return account;
    }

    public async Task<decimal> CheckBalance()
    {
        if (_account is null)
            throw new InvalidOperationException("Account not logged in.");
        await Task.Delay(500);
        return _account.Balance;
    }

    public async Task<decimal> CheckCredit()
    {
        if (_account is null)
            throw new InvalidOperationException("Account not logged in.");
        if (_account is not CreditAccount creditAccount)
            throw new InvalidOperationException("This account is not a credit account.");
        await Task.Delay(500);
        return creditAccount.Credit;
    }

    public async IAsyncEnumerable<string> CheckLogs()
    {
        if (_account is null)
            throw new InvalidOperationException("Account not logged in.");
        await Task.Delay(1000);
        foreach (var log in _account.Logs)
            yield return log;
    }

    public async Task DepositAsync(decimal amount)
    {
        if (_account is null)
            throw new InvalidOperationException("Account not logged in.");
        await Task.Delay(1000);
        if (new Random().Next(0, 10) < 3)
            throw new BadCashException();
        _account.Deposit(amount);
    }

    public async Task WithDrawal(decimal amount)
    {
        if (_account is null)
            throw new InvalidOperationException("Account not logged in.");
        if (amount >= BigMoneyLimit)
        {
            BigMoneyFetched?.Invoke(this, new(_account, amount));
            await Task.Delay(2000);
        }
        await Task.Delay(3000);
        _account.WithDrawal(amount);
    }

    #endregion Public Methods

    #region Public Classes

    public class BigMoneyEventArgs : EventArgs
    {
        #region Public Constructors

        public BigMoneyEventArgs(Account account, decimal amount)
        {
            Account = account;
            Amount = amount;
        }

        #endregion Public Constructors

        #region Public Properties

        public Account Account { get; init; }
        public decimal Amount { get; init; }

        #endregion Public Properties
    }

    #endregion Public Classes

    #region Private Fields

    readonly Atm _atm;
    Account? _account;

    #endregion Private Fields
}
