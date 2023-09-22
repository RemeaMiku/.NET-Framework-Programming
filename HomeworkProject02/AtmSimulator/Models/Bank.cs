using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AtmSimulator.Models;

public class Bank
{
    #region Public Constructors

    public Bank(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 30)
            throw new ArgumentException($"\"{name}\" is not valid name of a bank.");
        Name = name;
    }

    #endregion Public Constructors

    #region Public Properties

    public string Name { get; init; }

    #endregion Public Properties

    #region Public Indexers

    public Account? this[string phoneNumber]
    {
        get => _accounts.Find(a => a.PhoneNumber == phoneNumber);
    }

    #endregion Public Indexers

    #region Public Methods

    public Account AddAccount(string phoneNumber, string nameOfHolder, string password)
    {
        if (_accounts.Exists(a => a.PhoneNumber == phoneNumber))
            throw new ArgumentException("This phone number has been registered.");
        var accountId = _accounts.Count + 1;
        try
        {
            var account = new Account(accountId, phoneNumber, nameOfHolder, password, this);
            _accounts.Add(account);
            return account;
        }
        catch (ArgumentException e)
        {
            throw e;
        }
    }

    public Account AddCreditAccount(string phoneNumber, string nameOfHolder, string password, decimal creditLimit)
    {
        if (_accounts.Exists(a => a.PhoneNumber == phoneNumber))
            throw new ArgumentException("This phone number has been registered.");
        var accountId = _accounts.Count + 1;
        try
        {
            var account = new CreditAccount(accountId, phoneNumber, nameOfHolder, password, this, creditLimit);
            _accounts.Add(account);
            return account;
        }
        catch (ArgumentException e)
        {
            throw e;
        }
    }

    #endregion Public Methods

    #region Private Fields

    private readonly List<Account> _accounts = new();

    #endregion Private Fields
}
