using System;
using System.Windows;
using AtmSimulator.Models;
using AtmSimulator.Services;
using AtmSimulator.ViewModels;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;

namespace AtmSimulator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        var snackbarService = new SnackbarService();
        snackbarService.SetSnackbarControl(Snackbar);
        ViewModel = new(new(CreateAtm()), snackbarService);
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }

    #endregion Public Properties

    #region Private Methods

    private static Atm CreateAtm()
    {
        var random = new Random();
        var atm = new Atm();
        atm.Banks.Add(new Bank("中国银行"));
        atm.Banks.Add(new Bank("中国工商银行"));
        atm.Banks.Add(new Bank("中国农业银行"));
        atm.Banks.Add(new Bank("中国建设银行"));
        atm.Banks.Add(new Bank("中国天地银行"));
        atm.Banks[0].AddAccount("13814602632", "段红韶", "123456").Deposit(random.Next(1, 10) * 100M);
        atm.Banks[0].AddCreditAccount("19939504144", "贾连瑾", "123456", 1000M);
        atm.Banks[0].AddCreditAccount("18318084776", "荣颂岳", "123456", 10000M).Deposit(random.Next(1, 10) * 100M);
        atm.Banks[1].AddAccount("17787285073", "任罡晨", "123456");
        atm.Banks[1].AddAccount("15677076379", "鲁漫嵘", "123456").Deposit(random.Next(1, 10) * 100M);
        atm.Banks[1].AddCreditAccount("15776959154", "倪榕朗", "123456", 3000M);
        atm.Banks[2].AddAccount("13193796929", "郜国朔", "123456");
        atm.Banks[2].AddCreditAccount("13384715523", "阮灿炯", "123456", 5000M).Deposit(random.Next(1, 10) * 100M);
        atm.Banks[3].AddAccount("13049289649", "晏盼钊", "123456");
        atm.Banks[3].AddCreditAccount("15371642879", "潘笛萌", "123456", 0M);
        atm.Banks[4].AddAccount("15648432750", "吕若知", "123456").Deposit(random.Next(1, 10) * 100M);
        atm.Banks[4].AddAccount("13519924011", "吉妤贞", "123456");
        atm.Banks[4].AddCreditAccount("18198528130", "韶元洵", "123456", 3000M).Deposit(random.Next(1, 10) * 100M);
        return atm;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = (sender as PasswordBox)!.Password;
    }

    #endregion Private Methods


}
