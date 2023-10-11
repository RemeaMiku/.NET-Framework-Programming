using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Explorer.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace Explorer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Public Properties

    public new static App Current => (App)Application.Current;

    public IServiceProvider ServiceProvider { get; }
        = new ServiceCollection()
        .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
        .AddSingleton<ISnackbarService>(new SnackbarService())
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    #endregion Public Properties

    #region Protected Methods

    protected override void OnStartup(StartupEventArgs e) => ServiceProvider.GetRequiredService<MainWindow>().Show();

    #endregion Protected Methods
}
