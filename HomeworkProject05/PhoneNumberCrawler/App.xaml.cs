using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PhoneNumberCrawler;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Public Properties

    public new static App Current => (App)Application.Current;

    public IServiceProvider ServiceProvider { get; }
    = new ServiceCollection()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    #endregion Public Properties

    #region Protected Methods

    protected override void OnStartup(StartupEventArgs e)
        => ServiceProvider.GetRequiredService<MainWindow>().Show();

    #endregion Protected Methods
}
