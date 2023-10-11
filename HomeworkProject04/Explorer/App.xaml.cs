﻿using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Explorer.ViewModels;

namespace Explorer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 获取当前 App 实例
    /// </summary>
    public new static App Current => (App)Application.Current;
    /// <summary>
    /// 获取存放应用服务的容器
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
        = new ServiceCollection()
        .AddSingleton<ISnackbarService, SnackbarService>()
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();
    protected override void OnStartup(StartupEventArgs e) => ServiceProvider.GetRequiredService<MainWindow>().Show();
}
