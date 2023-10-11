using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Explorer.ViewModels;
using System.Windows.Controls;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace Explorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(ISnackbarService snackbarService)
    {
        InitializeComponent();
        DataContext = this;
        snackbarService.SetSnackbarControl(Snackbar);
        ViewModel = new(snackbarService);
    }

    public MainWindowViewModel ViewModel { get; }
}
