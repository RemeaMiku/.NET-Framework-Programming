using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentManagemantSystem.Data;
using StudentManagemantSystem.Services;
using StudentManagemantSystem.ViewModels;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace StudentManagemantSystem;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
        .AddSingleton<ISnackbarService, SnackbarService>()
        .AddSingleton<StudentManagementDbService>()
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    protected override void OnStartup(StartupEventArgs e)
        => ServiceProvider.GetRequiredService<MainWindow>().Show();
}
