using System.Windows;

namespace ProblemMaker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Public Constructors

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; } = new(new());

    #endregion Public Properties

    #region Private Methods

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsStarted))
        {
            if (ViewModel.IsStarted)
            {
                TargetCountBox.IsReadOnly = true;
                TargetCountBox.BorderThickness = new();
                StartButton.Content = "停止";
                ProblemPanel.Visibility = Visibility.Visible;
            }
            else
            {
                TargetCountBox.IsReadOnly = false;
                TargetCountBox.BorderThickness = new(1);
                StartButton.Content = "开始";
                ProblemPanel.Visibility = Visibility.Collapsed;
            }
        }
        if (e.PropertyName == nameof(MainWindowViewModel.IsSubmited))
        {
            if (ViewModel.IsSubmited)
            {
                SubmitButton.Content = "继续";
            }
            else
            {
                SubmitButton.Content = "提交";
            }
        }
    }

    #endregion Private Methods
}
