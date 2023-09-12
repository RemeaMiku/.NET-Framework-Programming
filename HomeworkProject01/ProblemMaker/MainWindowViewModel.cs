using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProblemMaker;

public partial class MainWindowViewModel : ObservableObject
{
    #region Public Constructors

    public MainWindowViewModel(ProblemService problemService)
    {
        _problemService = problemService;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTicked;
    }

    #endregion Public Constructors

    #region Private Fields

    private const int _maxTickCount = 15;

    private const string _questionGlyph = "\uF142";

    private const string _checkGlyph = "\uE73E";

    private const string _errorGlyph = "\uEDAE";

    private readonly static Brush _questionBrush = new SolidColorBrush(Colors.Black);

    private readonly static Brush _checkBrush = new SolidColorBrush(Colors.Green);

    private readonly static Brush _errorBrush = new SolidColorBrush(Colors.HotPink);

    private readonly ProblemService _problemService;
    private readonly DispatcherTimer _timer = new();

    [ObservableProperty]
    private int _tickCount = _maxTickCount;

    [ObservableProperty]
    Problem? _problem;

    [ObservableProperty]
    string _statusText = _questionGlyph;

    [ObservableProperty]
    string _answerText = string.Empty;

    [ObservableProperty]
    string _countText = "39";

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
    Brush _statusForeground = _questionBrush;

    #endregion Private Fields

    #region Private Methods

    private void OnTimerTicked(object? sender, EventArgs e)
    {
        TickCount--;
        if (TickCount == 0)
        {
            MessageBox.Show("超时，跳过此题", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText = _errorGlyph;
            StatusForeground = _errorBrush;
            IsSubmited = true;
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        _timer.Stop();
        TickCount = _maxTickCount;
    }
    [RelayCommand]
    void Start()
    {
        if (!IsStarted)
        {
            if (!int.TryParse(CountText, out var count))
            {
                MessageBox.Show("输入不合法，必须为整数", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (count < 1 || count > 999)
            {
                MessageBox.Show("必须在 [1,1000) 范围内", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        MessageBox.Show($"答题结束，得分为{Point} / {Count}，正确率{100 * Point / Count}%", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("输入不合法，必须为整数", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Problem!.CheckAnswer(answer))
            {
                StatusText = _checkGlyph;
                StatusForeground = _checkBrush;
                Point++;
            }
            else
            {
                StatusText = _errorGlyph;
                StatusForeground = _errorBrush;
            }
            IsSubmited = true;
            ResetTimer();
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
            StatusText = _questionGlyph;
            StatusForeground = _questionBrush;
            AnswerText = string.Empty;
            IsSubmited = false;
        }
    }

    #endregion Private Methods
}
