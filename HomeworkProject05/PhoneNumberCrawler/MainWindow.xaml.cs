using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace PhoneNumberCrawler;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{
    #region Public Constructors

    public MainWindow()
    {
        InitializeComponent();
        if (Theme.GetSystemTheme() == SystemThemeType.Dark)
            OnThemeToggleButtonClick(this, new());
        else
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
    }

    #endregion Public Constructors

    #region Public Methods

    public static Point GetMouseScreenPosition(Window window)
    {
        var result = Mouse.GetPosition(window);
        result.X += window.Left;
        result.Y += window.Top;
        return result;
    }

    #endregion Public Methods

    #region Private Fields

    const SymbolRegular _lightModeIcon = SymbolRegular.WeatherSunny24;

    const SymbolRegular _darkModeIcon = SymbolRegular.WeatherMoon24;

    private bool _restoreForDragMove;

    #endregion Private Fields

    #region Private Methods

    private void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        if (OptionsPanel.Width == 0)
        {
            var animation = new DoubleAnimation(300, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
            };
            OptionsPanel.BeginAnimation(WidthProperty, animation);
        }
        else
        {
            var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
            };
            OptionsPanel.BeginAnimation(WidthProperty, animation);
        }
    }
    private void OnThemeToggleButtonClick(object sender, RoutedEventArgs e)
    {
        if (Theme.GetAppTheme() == ThemeType.Light)
        {
            Theme.Apply(ThemeType.Dark, WindowBackdropType, true, true);
            ThemeButton.Icon = _lightModeIcon;
        }
        else
        {
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
            ThemeButton.Icon = _darkModeIcon;
        }
    }

    private void OnTitleBarMouseLeftClicked(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip)
                return;
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            _restoreForDragMove = WindowState == WindowState.Maximized;
            DragMove();
        }
    }

    private void OnTitleBarMouseMove(object sender, MouseEventArgs e)
    {
        if (_restoreForDragMove)
        {
            _restoreForDragMove = false;
            var point = PointToScreen(e.MouseDevice.GetPosition(this));
            Left = point.X - (RestoreBounds.Width * 0.5);
            Top = point.Y;
            WindowState = WindowState.Normal;
            DragMove();
        }
    }
    private void OnTitleBarMouseUp(object sender, MouseButtonEventArgs e)
        => _restoreForDragMove = false;


    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        => Close();

    private void OnUiWindowStateChanged(object sender, EventArgs e)
        => MaximizeButton.Icon = WindowState == WindowState.Normal ? SymbolRegular.Maximize24 : SymbolRegular.SquareMultiple24;
    private void OnTitleBarMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        => SystemCommands.ShowSystemMenu(this, GetMouseScreenPosition(this));

    #endregion Private Methods
}
