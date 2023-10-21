using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneNumberCrawler.Services;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace PhoneNumberCrawler.ViewModels;

public partial class MainWindowViewModel : ObservableValidator
{

    #region Public Constructors

    public MainWindowViewModel(BingPhoneNumberCrawlerService bingPhoneNumberCrawlerService, ISnackbarService snackbarService)
    {
        _bingPhoneNumberCrawlerService = bingPhoneNumberCrawlerService;
        _snackbarService = snackbarService;
        RegisterEvents();
    }

    #endregion Public Constructors

    #region Public Properties

    public string? SelectedPhoneNumber
    {
        get => _selectedPhoneNumber;
        set
        {
            if (_selectedPhoneNumber != value)
            {
                OnPropertyChanging(nameof(SelectedPhoneNumber));
                _selectedPhoneNumber = value;
                OnPropertyChanged(nameof(SelectedPhoneNumber));
                OnSelectedPhoneNumberChanged();
            }
        }
    }

    public int ProgressAsPercentage => 100 * Progress / TargetCount;

    public ObservableCollection<string> PhoneNumbers { get; } = new();

    public ObservableCollection<string> Souces { get; } = new();

    public List<string> UrlHistory { get; private set; } = new();

    public bool IsNotBusy => !IsBusy;

    #endregion Public Properties

    #region Private Fields

    const string _readyText = "准备就绪";

    readonly BingPhoneNumberCrawlerService _bingPhoneNumberCrawlerService;

    readonly ISnackbarService _snackbarService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    [ObservableProperty]
    bool _hasSearched = false;

    [Range(1, 999)]
    [ObservableProperty]
    int _targetCount = 100;

    [Range(1, 999_999)]
    [ObservableProperty]
    int _maxUrlCount = 100_000;

    [ObservableProperty]
    string _keyword = string.Empty;

    string? _selectedPhoneNumber;

    [ObservableProperty]
    string _infoText = _readyText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressAsPercentage))]
    int _progress = 0;

    #endregion Private Fields

    #region Private Methods

    void Reset()
    {
        SelectedPhoneNumber = default;
        PhoneNumbers.Clear();
        Souces.Clear();
        UrlHistory.Clear();
        HasSearched = false;
        Progress = 0;
        InfoText = _readyText;
    }

    [RelayCommand]
    void Cancel()
    {
        _bingPhoneNumberCrawlerService.Cancel();
        _snackbarService.Show("提示", $"正在取消搜索", SymbolRegular.Info24, ControlAppearance.Info);
    }

    void RegisterEvents()
    {
        _bingPhoneNumberCrawlerService.UriChanged += OnUriChanged;
        _bingPhoneNumberCrawlerService.NewPhoneNumberSearched += OnNewPhoneNumberSearched;
        _bingPhoneNumberCrawlerService.SourcesUpdated += OnSourcesUpdated;
    }

    private void OnSourcesUpdated(object? sender, BingPhoneNumberCrawlerService.SourcesUpdatedEventArgs e)
    {
        if (SelectedPhoneNumber == e.PhoneNumber)
            App.Current.Dispatcher.Invoke(() => Souces.Add(e.NewSource.AbsoluteUri));
    }

    private void OnNewPhoneNumberSearched(object? sender, BingPhoneNumberCrawlerService.NewPhoneNumberSearchedEventArgs e)
    {
        Progress = e.Progress;
        App.Current.Dispatcher.Invoke(() => PhoneNumbers.Add(e.PhoneNumber));
    }

    private void OnUriChanged(object? sender, BingPhoneNumberCrawlerService.UriChangedEventArgs e)
    {
        InfoText = e.CurrentUri.AbsoluteUri;
        App.Current.Dispatcher.Invoke(() => UrlHistory.Add(e.CurrentUri.AbsoluteUri));
    }

    [RelayCommand]
    async Task SearchAsync()
    {
        Reset();
        try
        {
            IsBusy = true;
            HasSearched = true;
            InfoText = $"正在通过Bing搜索：\"{Keyword}\"";
            await Task.Run(() => _bingPhoneNumberCrawlerService.Reset().Search(Keyword, TargetCount, MaxUrlCount)?.ToList());
            if (_bingPhoneNumberCrawlerService.Exception is not null)
                _snackbarService.Show("错误", $"发生了错误，已停止搜索：{_bingPhoneNumberCrawlerService.Exception.Message}", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            else
                _snackbarService.Show("完成", $"搜索已结束", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            _snackbarService.Show("错误", $"发生了错误，已停止搜索：{ex.Message}", SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            InfoText = _readyText;
        }
    }
    void OnSelectedPhoneNumberChanged()
    {
        Souces.Clear();
        if (SelectedPhoneNumber is null)
            return;
        foreach (var uri in _bingPhoneNumberCrawlerService.GetSources(SelectedPhoneNumber))
            Souces.Add(uri.AbsoluteUri);
    }

    #endregion Private Methods

}
