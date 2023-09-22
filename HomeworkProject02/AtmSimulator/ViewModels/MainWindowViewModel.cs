using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AtmSimulator.Exceptions;
using AtmSimulator.Models;
using AtmSimulator.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace AtmSimulator.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    readonly AtmService _atmService;
    readonly ISnackbarService _snackbarService;
    [ObservableProperty]
    List<string> _bankNames;

    public MainWindowViewModel(AtmService atmService, ISnackbarService snackbarService)
    {
        _atmService = atmService;
        _atmService.BigMoneyFetched += OnBigMoneyFetched;
        _snackbarService = snackbarService;
        _bankNames = _atmService.GetBankNames().ToList();
        BankName = _bankNames.First();
    }

    private void OnBigMoneyFetched(object? sender, AtmService.BigMoneyEventArgs e)
    {
        _snackbarService.Show("大金额提现警告", "将会比一般金额花费更长处理时间，请耐心等待", SymbolRegular.Warning28, ControlAppearance.Caution);
    }

    [ObservableProperty]
    string _phoneNumber = string.Empty;

    public string Password { get; set; } = string.Empty;

    [ObservableProperty]
    string _bankName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoginPanelVisibility))]
    bool _hasLogined;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressbarVisibility))]
    [NotifyPropertyChangedFor(nameof(OperationPanelVisibility))]
    bool _isBusy;
    public Visibility ProgressbarVisibility => IsBusy ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoginPanelVisibility => HasLogined ? Visibility.Collapsed : Visibility.Visible;

    public Visibility OperationPanelVisibility => HasLogined ? Visibility.Visible : Visibility.Collapsed;

    public Visibility CreditVisibility => IsCreditAccount ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LogsVisibility => DisplayLogs ? Visibility.Visible : Visibility.Collapsed;
    public string AccountType => IsCreditAccount ? "信用账户" : "普通账户";

    [ObservableProperty]
    string _nameOfAccount = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountType))]
    [NotifyPropertyChangedFor(nameof(CreditVisibility))]
    bool _isCreditAccount;

    [ObservableProperty]
    decimal _balance;

    [ObservableProperty]
    decimal _credit;

    [ObservableProperty]
    string _operation = "存款";

    [ObservableProperty]
    string _amountText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogsVisibility))]
    bool _displayLogs;

    [ObservableProperty]
    ObservableCollection<string> _logs = new();

    [RelayCommand]
    async Task Login()
    {
        try
        {
            IsBusy = true;
            var account = await _atmService.LoginAsync(BankName, PhoneNumber, Password);
            if (account is not null)
            {
                HasLogined = true;
                NameOfAccount = account.NameOfHolder;
                Balance = account.Balance;
                if (account is CreditAccount creditAccount)
                {
                    IsCreditAccount = true;
                    Credit = creditAccount.Credit;
                }
                account.Logs.ForEach(l => Logs.Add(l));
                _snackbarService.Show("登录成功", $"欢迎{account.NameOfHolder}先生/女士", SymbolRegular.Checkmark24, ControlAppearance.Success);
            }
        }
        catch (ArgumentException e)
        {
            _snackbarService.Show("登录失败", e.Message, SymbolRegular.Warning28, ControlAppearance.Caution);
        }
        catch (Exception e)
        {
            _snackbarService.Show("程序错误", e.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    async Task Operate()
    {
        try
        {
            IsBusy = true;
            if (!decimal.TryParse(AmountText, out var amount))
                throw new ArgumentException($"\"{AmountText}\"不是一个合法数字");
            if (AmountText.Contains('.'))
            {
                var values = AmountText.Split('.');
                if (values[1].Length > 2)
                {
                    _snackbarService.Show("小数过长", "金额将自动保留到2位小数", SymbolRegular.Warning28, ControlAppearance.Caution);
                    amount = Math.Round(amount, 2);
                    AmountText = amount.ToString();
                }
            }
            if (Operation == "存款")
                await _atmService.DepositAsync(amount);
            else
                await _atmService.WithDrawal(amount);
            _snackbarService.Show("操作成功", $"成功{Operation}{amount}元", SymbolRegular.Checkmark24, ControlAppearance.Success);
            Balance = await _atmService.CheckBalance();
            if (IsCreditAccount)
                Credit = await _atmService.CheckCredit();
            Logs = new();
            await foreach (var log in _atmService.CheckLogs())
                Logs.Add(log);
        }
        catch (ArgumentOutOfRangeException)
        {
            if (Operation == "存款")
                _snackbarService.Show("操作失败", "存取金额应为正数", SymbolRegular.Warning28, ControlAppearance.Caution);
            else
                _snackbarService.Show("操作失败", "提取金额应为正数且不大于余额", SymbolRegular.Warning28, ControlAppearance.Caution);
        }
        catch (ArgumentException e)
        {
            _snackbarService.Show("操作失败", e.Message, SymbolRegular.Warning28, ControlAppearance.Caution);
        }
        catch (BadCashException)
        {
            _snackbarService.Show("操作失败", "检测到坏钞，已退回检查并重试", SymbolRegular.Warning28, ControlAppearance.Caution);
        }
        catch (Exception e)
        {
            _snackbarService.Show("程序错误", e.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            AmountText = string.Empty;
        }
    }
}
