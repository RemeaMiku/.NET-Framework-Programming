using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PhoneNumberCrawler.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public IServiceProvider ServiceProvider { get; }
        = new ServiceCollection()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();
}
