using System.Windows;
using Wpf.Ui.Mvvm.Services;

namespace RandomQuestionGenerator;

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
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }


    #endregion Public Properties


    #region Private Methods

    private void OnDialogButtonLeftClick(object sender, RoutedEventArgs e) => Dialog.Hide();

    #endregion Private Methods
}
