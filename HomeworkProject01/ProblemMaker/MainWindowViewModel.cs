using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Common;
using System.Threading.Tasks;

namespace ProblemMaker;

public partial class MainWindowViewModel : ObservableObject
{
    #region Public Constructors    

    public MainWindowViewModel(ProblemService problemService, ISnackbarService snackbarService, IDialogService dialogService)
    {
        _problemService = problemService;
        _snackbarService = snackbarService;
        _dialogService = dialogService;
        _timer.Interval = TimeSpan.FromSeconds(0.1);
        _timer.Tick += OnTimerTicked;
    }

    #endregion Public Constructors

    #region Private Fields

    readonly ISnackbarService _snackbarService;
    readonly IDialogService _dialogService;
    readonly ProblemService _problemService;
    readonly DispatcherTimer _timer = new(DispatcherPriority.Normal);

    [ObservableProperty]
    Problem? _problem;

    [ObservableProperty]
    string _answerText = string.Empty;

    [ObservableProperty]
    string _countText = "10";

    [ObservableProperty]
    bool _isStarted = false;

    [ObservableProperty]
    bool _isSubmited = false;

    [ObservableProperty]
    int _point = 0;

    [ObservableProperty]
    int _count = 0;

    [ObservableProperty]
    int _index = 1;

    [ObservableProperty]
    SymbolRegular _submitButtonIcon = SymbolRegular.ArrowUpload24;

    #endregion Private Fields

    #region Private Methods

    void OnTimerTicked(object? sender, EventArgs e)
    {
        RemainingTime -= _timer.Interval;
        if (RemainingTime <= TimeSpan.Zero)
        {
            _dialogService.GetDialogControl().Show("警告", "超时，将跳过此题");
            IsSubmited = true;
            SubmitButtonIcon = SymbolRegular.Next24;
            ResetTimer();
        }
    }

    static readonly TimeSpan _maxTime = TimeSpan.FromSeconds(15);
    [ObservableProperty]
    TimeSpan _remainingTime = _maxTime;

    void ResetTimer()
    {
        _timer.Stop();
        RemainingTime = _maxTime;
    }
    [RelayCommand]
    void Start()
    {
        if (!IsStarted)
        {
            if (!int.TryParse(CountText, out var count))
            {
                _snackbarService.Show("警告", "目标题数输入不合法，应为整数", SymbolRegular.Warning28, ControlAppearance.Caution);
                return;
            }
            if (count < 1 || count > 999)
            {
                _snackbarService.Show("警告", "目标题数应在[1,1000)范围", SymbolRegular.Warning28, ControlAppearance.Caution);
                return;
            }
            Count = count;
            Problem = _problemService.Initialize(count);
            IsStarted = true;
            _timer.Start();
        }
        else
        {
            Stop();
        }
    }

    void Stop()
    {
        _dialogService.GetDialogControl().Show("提示", $"答题结束，得分为{Point} / {Count}，正确率{100 * Point / Count}%");
        IsStarted = false;
        Count = 39;
        Index = 1;
        ResetTimer();
    }

    [RelayCommand]
    void Submit()
    {
        if (!IsSubmited)
        {
            if (!int.TryParse(AnswerText, out var answer))
            {
                _snackbarService.Show("警告", "答案输入不合法，应为整数", SymbolRegular.Warning28, ControlAppearance.Caution);
                return;
            }
            IsSubmited = true;
            SubmitButtonIcon = SymbolRegular.Next24;
            ResetTimer();
            if (Problem!.CheckAnswer(answer))
            {
                _snackbarService.Show("提示", "回答正确", SymbolRegular.Checkmark24, ControlAppearance.Success);
                Point++;
            }
            else
            {
                _snackbarService.Show("提示", "回答错误", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            }
        }
        else
        {
            Index++;
            if (Index > Count)
            {
                Stop();
            }
            else
            {
                Problem = _problemService.GetNextProblem();
                _timer.Start();
            }
            AnswerText = string.Empty;
            IsSubmited = false;
            SubmitButtonIcon = SymbolRegular.ArrowUpload24;
        }
    }

    #endregion Private Methods
}
