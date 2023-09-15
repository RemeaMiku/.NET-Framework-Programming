using System.Windows;
using Wpf.Ui.Mvvm.Services;

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
        var snackbarService = new SnackbarService();
        snackbarService.SetSnackbarControl(AnswerFeedbackBar);
        var dialogService = new DialogService();
        dialogService.SetDialogControl(Dialog);
        ViewModel = new(new(), snackbarService, dialogService);
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }


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
                RemainingTimeLabel.Visibility = Visibility.Visible;
                RemainingTimeValueLabel.Visibility = Visibility.Visible;
            }
            else
            {
                TargetCountBox.IsReadOnly = false;
                TargetCountBox.BorderThickness = new(1);
                StartButton.Content = "开始";
                ProblemPanel.Visibility = Visibility.Collapsed;
                RemainingTimeLabel.Visibility = Visibility.Collapsed;
                RemainingTimeValueLabel.Visibility = Visibility.Collapsed;
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

    private void OnDialogButtonLeftClick(object sender, RoutedEventArgs e)
    {
        Dialog.Hide();
    }
}
