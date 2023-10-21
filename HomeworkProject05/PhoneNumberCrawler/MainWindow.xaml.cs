// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Windows;
using System.Windows.Media.Animation;
using PhoneNumberCrawler.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace PhoneNumberCrawler;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{
    #region Public Constructors

    public MainWindow(MainWindowViewModel viewModel, ISnackbarService snackbarService)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
        snackbarService.SetSnackbarControl(Snackbar);
        SetWindowTheme();
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }

    #endregion Public Properties

    #region Private Methods

    #region AppThemeManagement

    private void SetWindowTheme()
    {
        if (Theme.GetSystemTheme() == SystemThemeType.Dark)
            OnThemeToggleButtonClick(this, new());
        else
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
    }

    private void OnThemeToggleButtonClick(object sender, RoutedEventArgs e)
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

    #endregion AppThemeManagement

    #region ExpandPanelManagement

    private void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        if (ExpandPanel.Width == 0)
        {
            var animation = new DoubleAnimation(ExpandPanel.MaxWidth, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut },
            };
            ExpandPanel.BeginAnimation(WidthProperty, animation);
        }
        else
        {
            var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
            };
            ExpandPanel.BeginAnimation(WidthProperty, animation);
        }
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ExpandPanel.MaxWidth = 0.6 * e.NewSize.Width;
        if (ExpandPanel.Width != 0)
            OnExpandButtonClicked(sender, e);
    }

    #endregion ExpandPanelManagement

    private void OnNumberBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is NumberBox box)
            if (string.IsNullOrWhiteSpace(box.Text))
                box.Text = box.Minimum.ToString();
    }

    #endregion Private Methods
}