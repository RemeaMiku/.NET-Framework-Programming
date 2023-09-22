using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AtmSimulator.Models;

public partial class Account
{
    #region Public Properties

    public int AccountNumber { get; init; }

    public string PhoneNumber
    {
        get => _phoneNumber ?? throw new InvalidOperationException($"{nameof(PhoneNumber)} is null.");
        private set
        {
            ValidatePhoneNumber(value);
            _phoneNumber = value;
        }
    }

    public string NameOfHolder
    {
        get => _nameOfHolder ?? throw new InvalidOperationException($"{nameof(NameOfHolder)} is null.");
        private set
        {
            ValidateNameOfHolder(value);
            _nameOfHolder = value;
        }
    }

    public string Password
    {
        get => _password ?? throw new InvalidOperationException($"{nameof(Password)} is null.");
        private set
        {
            ValidatePassword(value);
            _password = value;
        }
    }

    public DateTimeOffset OpeningTime { get; }

    public Bank Bank { get; init; }

    public decimal Balance { get; protected set; }

    public List<string> Logs { get; } = new();

    #endregion Public Properties

    #region Public Methods

    public static void ValidatePhoneNumber(string phoneNumber)
    {
        if (!PhoneNumberRegex().IsMatch(phoneNumber))
            throw new ArgumentException($"The value is not a valid phone number.");
    }

    public static void ValidatePassword(string password)
    {
        if (password.Length != 6)
            throw new ArgumentException($"The value is not a valid password.");
        foreach (var ch in password)
            if (!char.IsNumber(ch))
                throw new ArgumentException($"The value is not a valid password.");
    }

    public static void ValidateNameOfHolder(string nameOfHolder)
    {
        if (nameOfHolder.Length > 20)
            throw new ArgumentException($"The value is not a valid name of holder.");
    }

    public virtual void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "The deposit amount should be greater than 0.");
        checked
        {
            Balance += amount;
        }
        Log(true, amount);
    }

    public virtual void WithDrawal(decimal amount)
    {
        if (amount <= 0 || amount > Balance)
            throw new ArgumentOutOfRangeException(nameof(amount), "The withdrawal amount should be greater than 0 and not greater than the balance.");
        Balance -= amount;
        Log(false, amount);
    }

    #endregion Public Methods

    #region Internal Constructors

    internal Account(int accountNumber, string phoneNumber, string nameOfHolder, string password, Bank bank)
    {
        AccountNumber = accountNumber;
        PhoneNumber = phoneNumber;
        NameOfHolder = nameOfHolder;
        Password = password;
        Bank = bank;
        OpeningTime = DateTimeOffset.Now;
    }

    #endregion Internal Constructors

    #region Protected Methods

    protected void Log(bool isDeposit, decimal amount)
    {
        var sign = isDeposit ? '+' : '-';
        Logs.Add($"时间：{DateTimeOffset.Now:yyyy.MM.dd-HH:mm:ss} 操作：{sign} {amount}元 操作后余额：{Balance}元");
    }

    #endregion Protected Methods

    #region Private Fields

    string? _phoneNumber;
    string? _nameOfHolder;
    /// <summary>
    /// 密码应该要加密处理，这里简化直接用字符串
    /// </summary>
    string? _password;

    #endregion Private Fields

    #region Private Methods

    [GeneratedRegex("^[1]+[3,4,5,7,8,9]+\\d{9}")]
    private static partial Regex PhoneNumberRegex();

    #endregion Private Methods
}

