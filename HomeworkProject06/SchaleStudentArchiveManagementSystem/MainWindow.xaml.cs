﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SchaleStudentArchiveManagementSystem.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace SchaleStudentArchiveManagementSystem;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{

    #region Public Constructors

    public MainWindow(ISnackbarService snackbarService, MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
        snackbarService.SetSnackbarControl(Snackbar);
        _navigateButtons.Add(StudentsNavigateButton);
        _navigateButtons.Add(ClassesNavigateButton);
        _navigateButtons.Add(SchoolsNavigateButton);
        Loaded += (sender, e) =>
        {
            OnThemeButtonClicked(sender, e);
            OnNavigateButtonClicked(StudentsNavigateButton, new());
        };
        Closing += (sender, e) => ViewModel.DisconnectFromDatabase();
        //连接数据库。因时间有限，没有做登录连接界面，在这里直接执行命令
        //路径为当前路径的BlueArchive.db
        ViewModel.DbPath = Path.Combine(Environment.CurrentDirectory, "BlueArchive.db");
        ViewModel.ConnectToDatabaseCommand.Execute(null);
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }

    #endregion Public Properties

    #region Private Fields

    readonly List<Button> _navigateButtons = new();

    #endregion Private Fields

    #region Private Methods

    void OnDataGridAutoGeneratingColumn(object sender, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
    }

    void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        foreach (var button in _navigateButtons)
        {
            button.IconFilled = false;
            button.Background = Brushes.Transparent;
            button.Foreground = (Brush)App.Current.Resources["TextFillColorPrimaryBrush"];
        }
        if (sender is Button currentButton)
        {
            currentButton.Background = currentButton.MouseOverBackground;
            var animation = new ColorAnimation(Accent.SystemAccent, TimeSpan.FromSeconds(0.2));
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(currentButton, animation);
            Storyboard.SetTargetProperty(animation, new("Foreground.Color"));
            storyboard.Begin(currentButton);
            currentButton.IconFilled = true;
            ViewModel.EntityTypeName = (string)currentButton.Tag;
        }
    }

    void OnDataGridAutoGeneratedColumns(object sender, EventArgs e)
    {
        DataGrid.Columns[0].IsReadOnly = true;
        foreach (var column in DataGrid.Columns)
        {
            var header = (string)column.Header;
            if (header.Contains("集合"))
                column.Visibility = Visibility.Collapsed;
            else if (header.Contains("所属") && header.Contains("名称"))
                column.IsReadOnly = true;
        }
    }

    void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => ViewModel.SelectedItems = DataGrid.SelectedItems;

    void OnThemeButtonClicked(object sender, RoutedEventArgs e)
    {
        if (Theme.GetAppTheme() == ThemeType.Light)
        {
            Theme.Apply(ThemeType.Dark, WindowBackdropType, true, true);
            ThemeButton.Icon = SymbolRegular.WeatherSunny24;
        }
        else
        {
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
            ThemeButton.Icon = SymbolRegular.WeatherMoon24;
        }
    }

    #endregion Private Methods

}
