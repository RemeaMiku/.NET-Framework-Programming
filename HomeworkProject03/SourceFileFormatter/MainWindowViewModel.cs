using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace SourceFileFormatter;

public partial class MainWindowViewModel : ObservableObject
{
    #region Public Fields

    [ObservableProperty]
    public string _formattedText = string.Empty;

    #endregion Public Fields

    #region Public Constructors

    public MainWindowViewModel(SourceFileFormatService sourceFileFormatService, ISnackbarService snackbarService)
    {
        _sourceFileFormatService = sourceFileFormatService;
        _snackbarService = snackbarService;
    }

    #endregion Public Constructors

    #region Public Properties

    public Visibility ImportPanelVisibility => IsRead ? Visibility.Collapsed : Visibility.Visible;

    public Visibility StatisticsPanelVisibility => !IsRead ? Visibility.Collapsed : Visibility.Visible;

    public Visibility ProgressBarVisibility => !IsBusy ? Visibility.Collapsed : Visibility.Visible;

    public Visibility WordStatisticsVisibility => !IsDisplayingWordStatistics ? Visibility.Collapsed : Visibility.Visible;

    public ObservableCollection<KeyValuePair<string, int>> WordStatistics { get; } = new();

    #endregion Public Properties

    #region Private Fields

    readonly SourceFileFormatService _sourceFileFormatService;
    readonly ISnackbarService _snackbarService;
    [ObservableProperty]
    string _filePath = "将文件拖拽到此或点击选择文件";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImportPanelVisibility))]
    [NotifyPropertyChangedFor(nameof(StatisticsPanelVisibility))]
    bool _isRead = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressBarVisibility))]
    bool _isBusy = false;

    [ObservableProperty]
    bool _isDisplayingWordStatistics = false;

    [ObservableProperty]
    int _wordCount;

    [ObservableProperty]
    int _lineCount;
    [ObservableProperty]
    CSharpSourceFileFormatter? _formatter;

    #endregion Private Fields

    #region Private Methods

    [RelayCommand]
    async Task ReadAsync()
    {
        if (FilePath == "将文件拖拽到此或点击选择文件")
        {
            _snackbarService.Show("读取文件出错", "未选择任何文件", SymbolRegular.Warning28, ControlAppearance.Danger);
            return;
        }
        try
        {
            IsBusy = true;
            var formatter = await _sourceFileFormatService.GetFormatter(FilePath);
            if (formatter is not null)
            {
                IsRead = true;
                Formatter = formatter;
                LineCount = formatter.LineCount;
                WordCount = formatter.CountAllWords();
                foreach (var pair in formatter.WordCountDic)
                    WordStatistics.Add(pair);
            }
        }
        catch (Exception e)
        {
            _snackbarService.Show("读取文件出错", e.Message, SymbolRegular.Warning28, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    void Format()
    {
        if (Formatter is null)
            throw new InvalidOperationException();
        try
        {
            IsBusy = true;
            Formatter.RemoveEmptyLinesAndSingleLineComments();
            LineCount = Formatter.LineCount;
            WordCount = Formatter.CountAllWords();
            WordStatistics.Clear();
            foreach (var pair in Formatter.WordCountDic)
                WordStatistics.Add(pair);
            FormattedText = Formatter.Text;
        }
        catch (Exception e)
        {
            _snackbarService.Show("格式化出错", e.Message, SymbolRegular.Warning28, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Private Methods
}
