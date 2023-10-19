using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;

namespace PhoneNumberCrawler;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (Theme.GetSystemTheme() == SystemThemeType.Dark)
            OnThemeToggleButtonClick(this, new());
        else
            Theme.Apply(ThemeType.Light, BackgroundType.Auto, true, true);
    }

    private void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        if (OptionsPanel.Width == 0)
        {
            var animation = new DoubleAnimation(250, TimeSpan.FromSeconds(0.3))
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

    const SymbolRegular _lightModeIcon = SymbolRegular.WeatherSunny24;
    const SymbolRegular _darkModeIcon = SymbolRegular.WeatherMoon24;

    private void OnThemeToggleButtonClick(object sender, RoutedEventArgs e)
    {
        if (Theme.GetAppTheme() == ThemeType.Light)
        {
            Theme.Apply(ThemeType.Dark, BackgroundType.Auto, true, true);
            ThemeButton.Icon = _lightModeIcon;
        }
        else
        {
            Theme.Apply(ThemeType.Light, BackgroundType.Auto, true, true);
            ThemeButton.Icon = _darkModeIcon;
        }
    }
}
