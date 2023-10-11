using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Mvvm.Contracts;
using Explorer.ViewModels;
using System.Windows.Input;
using System.Diagnostics;
using System;
using Wpf.Ui.Appearance;

namespace Explorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors

    public MainWindow(ISnackbarService snackbarService)
    {
        InitializeComponent();
        DataContext = this;
        snackbarService.SetSnackbarControl(Snackbar);
        ViewModel = new(snackbarService);
        CloseAction = new(Close);
        RunNewInstanceAction = () => Process.Start(Environment.ProcessPath!);
        Theme.Apply(ThemeType.Light, BackgroundType.Auto, true, true);
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }

    public Action CloseAction { get; }

    public Action RunNewInstanceAction { get; }

    #endregion Public Properties

    #region Private Methods

    private void OnButtonEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var button = sender as System.Windows.Controls.Button;
        if (button is not null)
        {
            if (button.IsEnabled)
            {
                button.Cursor = Cursors.Arrow;
                button.Foreground = button.Tag as Brush;
            }
            else
            {
                button.Tag = button.Foreground;
                button.Cursor = Cursors.No;
                button.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
    }
    private void OnFileMenuItemClicked(object sender, RoutedEventArgs e) => ((sender as System.Windows.Controls.MenuItem)!.Tag as Action)?.Invoke();
    private void OnViewMenuItemChecked(object sender, RoutedEventArgs e)
    {
        var ui = (sender as System.Windows.Controls.MenuItem)!.Tag as UIElement;
        if (ui is not null)
        {
            ui.Visibility = Visibility.Visible;
            if (ui == NavigationView)
            {
                NavigationPanel.Width = (GridLength)NavigationPanel.Tag;
                NavigationPanel.MinWidth = 100;
            }
        }
    }
    private void OnViewMenuItemUnchecked(object sender, RoutedEventArgs e)
    {
        var ui = (sender as System.Windows.Controls.MenuItem)!.Tag as UIElement;
        if (ui is not null)
        {
            ui.Visibility = Visibility.Collapsed;
            if (ui == NavigationView)
            {
                NavigationPanel.MinWidth = 0;
                NavigationPanel.Tag = NavigationPanel.Width;
                NavigationPanel.Width = new(0);
            }
        }
    }
    private void OnOptionMenuItemChecked(object sender, RoutedEventArgs e)
        => Theme.Apply(ThemeType.Dark, BackgroundType.Auto, true, false);


    private void OnOptionMenuItemUnchecked(object sender, RoutedEventArgs e)
        => Theme.Apply(ThemeType.Light, BackgroundType.Auto, true, false);

    #endregion Private Methods
}
