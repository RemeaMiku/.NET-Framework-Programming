using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace SourceFileFormatter;

public partial class MainWindowViewModel : ObservableObject
{
    #region Public Constructors

    public MainWindowViewModel(SourceFileFormatService sourceFileFormatService, ISnackbarService snackbarService)
    {
        _sourceFileFormatService = sourceFileFormatService;
        _snackbarService = snackbarService;
    }

    #endregion Public Constructors

    #region Public Enums

    public enum WordSortMode
    {
        以先后排序,

        以数量升序,

        以数量降序,
    }

    #endregion Public Enums

    #region Public Properties

    public string Text
    {
        get
        {
            if (_formatter is null)
                return string.Empty;
            return _formatter.Text;
        }
    }

    public int LineCount
    {
        get
        {
            if (_formatter is null)
                return -1;
            return _formatter.LineCount;
        }
    }

    public int WordCount
    {
        get
        {
            if (_formatter is null)
                return -1;
            return _formatter.CountAllWords();
        }
    }

    public ObservableCollection<KeyValuePair<string, int>> WordStatistics { get; } = new();

    public bool IsNotRead => !IsRead;

    public WordSortMode SortMode
    {
        get => _sortMode;

        set
        {
            if (_sortMode == value)
                return;
            _sortMode = value;
            ReorderWordList();
        }
    }

    public IEnumerable<WordSortMode> WordSortModes { get; } = Enum.GetValues<WordSortMode>();

    #endregion Public Properties

    #region Private Fields

    private const string _tip = "将文件拖拽到此或点击选择文件";

    private readonly SourceFileFormatService _sourceFileFormatService;

    private readonly ISnackbarService _snackbarService;

    [ObservableProperty]
    private string _filePath = _tip;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRead))]
    private bool _isRead = false;

    [ObservableProperty]
    private bool _isBusy = false;

    private CSharpSourceFileFormatter? _formatter;

    private WordSortMode _sortMode;

    #endregion Private Fields

    #region Private Methods

    [RelayCommand]
    private async Task ReadAsync()
    {
        if (FilePath == _tip)
        {
            _snackbarService.Show("读取文件失败", "未选择任何文件", SymbolRegular.Warning28, ControlAppearance.Danger);
            return;
        }
        try
        {
            IsBusy = true;
            var formatter = await _sourceFileFormatService.GetFormatter(FilePath);
            if (formatter is not null)
            {
                _formatter = formatter;
                OnPropertyChanged(nameof(LineCount));
                OnPropertyChanged(nameof(WordCount));
                OnPropertyChanged(nameof(Text));
                ReorderWordList();
                IsRead = true;
            }
        }
        catch (Exception e)
        {
            _snackbarService.Show("读取文件失败", e.Message, SymbolRegular.Warning28, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Format()
    {
        if (_formatter is null)
            throw new InvalidOperationException();
        try
        {
            IsBusy = true;
            _formatter.RemoveEmptyLinesAndSingleLineComments();
            OnPropertyChanged(nameof(LineCount));
            OnPropertyChanged(nameof(WordCount));
            OnPropertyChanged(nameof(Text));
            WordStatistics.Clear();
            ReorderWordList();
        }
        catch (Exception e)
        {
            _snackbarService.Show("格式化失败", e.Message, SymbolRegular.Warning28, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReorderWordList()
    {
        if (_formatter is null)
            throw new InvalidOperationException("The formatter has not been initialized");
        switch (_sortMode)
        {
            case WordSortMode.以先后排序:
                ReloadWordList(_formatter.WordCountDic);
                break;

            case WordSortMode.以数量升序:
                ReloadWordList(_formatter.WordCountDic.OrderBy((pair) => pair.Value));
                break;

            case WordSortMode.以数量降序:
                ReloadWordList(_formatter.WordCountDic.OrderByDescending((pair) => pair.Value));
                break;

            default:
                break;
        }
    }

    private void ReloadWordList(IEnumerable<KeyValuePair<string, int>> source)
    {
        if (WordStatistics.Any())
            WordStatistics.Clear();
        foreach (var pair in source)
            WordStatistics.Add(pair);
    }

    [RelayCommand]
    private void Reselect()
    {
        _formatter = default;
        FilePath = _tip;
        IsRead = false;
    }

    #endregion Private Methods
}