using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Mvvm.Contracts;
using Explorer.ViewModels;
using System.Diagnostics;
using System;
using Wpf.Ui.Appearance;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace Explorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        App.Current.ServiceProvider.GetRequiredService<ISnackbarService>().SetSnackbarControl(Snackbar);
        ViewModel = viewModel;
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
    private void OnFileMenuItemClicked(object sender, RoutedEventArgs e)
        => ((sender as MenuItem)!.Tag as Action)?.Invoke();

    private void OnViewMenuItemChecked(object sender, RoutedEventArgs e)
    {
        var ui = (sender as MenuItem)!.Tag as UIElement;
        if (ui is not null)
        {
            if (ui == NavigationView)
            {
                NavigationPanel.Width = (GridLength)NavigationPanel.Tag;
                NavigationPanel.MinWidth = 100;
            }
        }
    }

    private void OnViewMenuItemUnchecked(object sender, RoutedEventArgs e)
    {
        var ui = (sender as MenuItem)!.Tag as UIElement;
        if (ui is not null)
        {
            if (ui == NavigationView)
            {
                NavigationPanel.MinWidth = 0;
                NavigationPanel.Tag = NavigationPanel.Width;
                NavigationPanel.Width = new(0);
            }
        }
    }

    private void OnOptionMenuItemChecked(object sender, RoutedEventArgs e)
    {
        Panel.SetZIndex(Mask, int.MaxValue);
        var whiteToBlackAnimation = new ColorAnimation(Colors.White, Colors.Black, new(TimeSpan.FromSeconds(0.2)));
        var fadeInOutAnimation = new DoubleAnimation(0, 100, new(TimeSpan.FromSeconds(0.1)))
        {
            AutoReverse = true
        };
        var storyboard = new Storyboard();
        Storyboard.SetTargetName(whiteToBlackAnimation, nameof(Mask));
        Storyboard.SetTargetProperty(whiteToBlackAnimation, new("Fill.Color"));
        Storyboard.SetTargetName(fadeInOutAnimation, nameof(Mask));
        Storyboard.SetTargetProperty(fadeInOutAnimation, new("Opacity"));
        storyboard.Children.Add(fadeInOutAnimation);
        storyboard.Children.Add(whiteToBlackAnimation);
        storyboard.Begin(Mask);
        Theme.Apply(ThemeType.Dark, BackgroundType.Auto, true, true);
        Panel.SetZIndex(Mask, -1);
    }


    private void OnOptionMenuItemUnchecked(object sender, RoutedEventArgs e)
    {
        Panel.SetZIndex(Mask, int.MaxValue);
        var blackToWhiteAnimation = new ColorAnimation(Colors.Black, Colors.White, new(TimeSpan.FromSeconds(0.2)));
        var fadeInOutAnimation = new DoubleAnimation(0, 100, new(TimeSpan.FromSeconds(0.1)))
        {
            AutoReverse = true
        };
        var storyboard = new Storyboard();
        Storyboard.SetTargetName(blackToWhiteAnimation, nameof(Mask));
        Storyboard.SetTargetProperty(blackToWhiteAnimation, new("Fill.Color"));
        Storyboard.SetTargetName(fadeInOutAnimation, nameof(Mask));
        Storyboard.SetTargetProperty(fadeInOutAnimation, new("Opacity"));
        storyboard.Children.Add(blackToWhiteAnimation);
        storyboard.Children.Add(fadeInOutAnimation);
        storyboard.Begin(Mask);
        Theme.Apply(ThemeType.Light, BackgroundType.Auto, true, true);
        Panel.SetZIndex(Mask, -1);
    }


    #endregion Private Methods
}
