using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Extensions;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Public Constructors

    public MainWindowViewModel(ISnackbarService snackbarService)
    {
        _snackbarService = snackbarService;
        var driveInfos = DriveInfo.GetDrives();
        foreach (var driveInfo in driveInfos)
            TreeViewItemViewModel.Root.Items.Add(new(driveInfo));
        Roots.Add(TreeViewItemViewModel.Root);
        WeakReferenceMessenger.Default.Register(this, "Expanding", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanding(m));
        WeakReferenceMessenger.Default.Register(this, "Expanded", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanded());
        WeakReferenceMessenger.Default.Register(this, "Selected", async (MainWindowViewModel r, TreeViewItemViewModel m) =>
        {
            await r.OnItemSelectedAsync(m);
        });
        TreeViewItemViewModel.Root.IsSelected = true;
    }

    #endregion Public Constructors

    #region Public Properties

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<TreeViewItemViewModel> Items { get; } = new();

    public ObservableCollection<TreeViewItemViewModel> Roots { get; } = new();

    #endregion Public Properties

    #region Private Fields

    const string _ready = "准备就绪";

    readonly ISnackbarService _snackbarService;
    readonly Stack<TreeViewItemViewModel> _backStack = new();

    readonly Stack<TreeViewItemViewModel> _forwardStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy;
    [ObservableProperty]
    string _statusInfo = _ready;
    [ObservableProperty]
    TreeViewItemViewModel _currentItemViewModel = TreeViewItemViewModel.Empty;
    [ObservableProperty]
    bool _isBackButtonEnabled;

    [ObservableProperty]
    bool _isForwordButtonEnabled;

    #endregion Private Fields

    #region Private Methods

    void OnTreeViewItemExpanding(TreeViewItemViewModel itemViewModel)
    {
        IsBusy = true;
        StatusInfo = $"正在展开：{itemViewModel.FullPath}";
    }

    void OnTreeViewItemExpanded()
    {
        IsBusy = false;
        StatusInfo = _ready;
    }

    void PushBackStack(TreeViewItemViewModel itemViewModel)
    {
        if (!IsBackButtonEnabled)
            IsBackButtonEnabled = true;
        if (itemViewModel != TreeViewItemViewModel.Empty)
            _backStack.Push(itemViewModel);
    }

    TreeViewItemViewModel PopBackStack()
    {
        var res = _backStack.Pop();
        if (_backStack.Count == 0)
            IsBackButtonEnabled = false;
        return res;
    }

    async Task OnItemSelectedAsync(TreeViewItemViewModel itemViewModel, bool calledFromMessage = true)
    {
        try
        {
            IsBusy = true;
            StatusInfo = $"正在加载：{itemViewModel.FullPath}";
            var info = itemViewModel.GetDirectoryInfo();
            if (info is null)
            {
                if (itemViewModel != TreeViewItemViewModel.Root)
                {
                    RunProcess(itemViewModel.FullPath);
                    return;
                }
                LoadDrives(itemViewModel);
            }
            else
                await LoadDirectoryAsync(info);
            if (calledFromMessage && CurrentItemViewModel != TreeViewItemViewModel.Empty)
                PushBackStack(CurrentItemViewModel);
            if (calledFromMessage && IsForwordButtonEnabled && itemViewModel.FullPath != PopForwordStack().FullPath)
                ClearForwordStack();
            CurrentItemViewModel = itemViewModel;
        }
        catch (Exception ex)
        {
            _snackbarService.Show("错误", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            StatusInfo = _ready;
        }
    }

    void RunProcess(string path)
    {
        StatusInfo = $"正在打开：{path}";
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.ErrorDialog = true;
        if (process.Start())
            _snackbarService.Show("提示", $"已打开文件：{path}。", SymbolRegular.Check24, ControlAppearance.Success);
    }

    void LoadDrives(TreeViewItemViewModel itemViewModel)
    {
        Items.Clear();
        foreach (var item in itemViewModel.Items)
            Items.Add(item);
    }

    async Task LoadDirectoryAsync(DirectoryInfo info)
    {
        Items.Clear();
        var viewModels = new List<TreeViewItemViewModel>();
        await Task.Run(() =>
        {
            foreach (var item in info.EnumerateAccessibleDirectories())
                viewModels.Add(new(item));
            foreach (var item in info.EnumerateAccessibleFiles())
                viewModels.Add(new(item));
        });
        foreach (var viewModel in viewModels)
            Items.Add(viewModel);
    }
    void PushForwordStack(TreeViewItemViewModel viewItemViewModel)
    {
        if (!IsForwordButtonEnabled)
            IsForwordButtonEnabled = true;
        _forwardStack.Push(viewItemViewModel);
    }

    void ClearForwordStack()
    {
        _forwardStack.Clear();
        IsForwordButtonEnabled = false;
    }

    TreeViewItemViewModel PopForwordStack()
    {
        var res = _forwardStack.Pop();
        if (_forwardStack.Count == 0)
            IsForwordButtonEnabled = false;
        return res;
    }

    [RelayCommand]
    async Task BackAsync()
    {
        PushForwordStack(CurrentItemViewModel);
        await OnItemSelectedAsync(PopBackStack(), false);
    }
    [RelayCommand]
    async Task ForwordAsync()
    {
        PushBackStack(CurrentItemViewModel);
        await OnItemSelectedAsync(PopForwordStack(), false);
    }

    #endregion Private Methods
}
