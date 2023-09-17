using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Common;
using System.Windows;

namespace RandomQuestionGenerator;

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

    #region Public Properties

    public bool IsEnded => !IsStarted;

    public Visibility QuestionVisibility => IsStarted ? Visibility.Visible : Visibility.Collapsed;

    public SymbolRegular StartEndButtonSymbol => IsStarted ? SymbolRegular.Stop24 : SymbolRegular.Play24;

    public string StartEndButtonContent => IsStarted ? "结束" : "开始";

    public string SubmitNextButtonContent => IsSubmited ? "继续" : "提交";

    public SymbolRegular SubmitNextButtonSymbol => IsSubmited ? SymbolRegular.Next24 : SymbolRegular.ArrowUpload24;

    #endregion Public Properties

    #region Private Fields

    const int _initCount = 10;
    static readonly TimeSpan _maxTime = TimeSpan.FromSeconds(15);

    readonly ISnackbarService _snackbarService;
    readonly IDialogService _dialogService;
    readonly ProblemService _problemService;
    readonly DispatcherTimer _timer = new(DispatcherPriority.Normal);

    [ObservableProperty]
    Question _question;

    [ObservableProperty]
    string _answerText = string.Empty;

    [ObservableProperty]
    string _countText = _initCount.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnded))]
    [NotifyPropertyChangedFor(nameof(QuestionVisibility))]
    [NotifyPropertyChangedFor(nameof(StartEndButtonSymbol))]
    [NotifyPropertyChangedFor(nameof(StartEndButtonContent))]
    bool _isStarted = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitNextButtonSymbol))]
    [NotifyPropertyChangedFor(nameof(SubmitNextButtonContent))]
    bool _isSubmited = false;
    [ObservableProperty]
    int _point = 0;

    int _count = 0;

    [ObservableProperty]
    int _index = 0;
    [ObservableProperty]
    TimeSpan _remainingTime = _maxTime;

    #endregion Private Fields

    #region Private Methods

    void OnTimerTicked(object? sender, EventArgs e)
    {
        RemainingTime -= _timer.Interval;
        if (RemainingTime <= TimeSpan.Zero)
        {
            _dialogService.GetDialogControl().Show("警告", "超时，将跳过此题");
            IsSubmited = true;
            ResetTimer();
        }
    }
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
            Index++;
            _count = count;
            Question = _problemService.Initialize(count);
            IsStarted = true;
            _timer.Start();
        }
        else
            Stop();
    }

    void Stop()
    {
        _dialogService.GetDialogControl().Show("提示", $"答题结束，得分为{Point} / {_count}，正确率{100 * Point / _count}%");
        AnswerText = string.Empty;
        IsStarted = false;
        _count = _initCount;
        Index = 0;
        Point = 0;
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
            ResetTimer();
            if (Question!.Check(answer))
            {
                _snackbarService.Show("提示", "回答正确", SymbolRegular.Checkmark24, ControlAppearance.Success);
                Point++;
            }
            else
                _snackbarService.Show("提示", "回答错误", SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        else
        {
            Index++;
            if (Index > _count)
                Stop();
            else
            {
                Question = _problemService.GetNextProblem();
                _timer.Start();
            }
            AnswerText = string.Empty;
            IsSubmited = false;
        }
    }

    #endregion Private Methods

}
