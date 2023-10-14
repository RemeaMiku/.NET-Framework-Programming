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
using Microsoft.Toolkit.Uwp.Notifications;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{

    public MainWindowViewModel(IMessenger messenger, ISnackbarService snackbarService)
    {
        _messenger = messenger;
        _snackbarService = snackbarService;
        var driveInfos = DriveInfo.GetDrives();
        foreach (var driveInfo in driveInfos)
            TreeViewItemViewModel.Root.Items.Add(new(driveInfo));
        Roots.Add(TreeViewItemViewModel.Root);
        RegisterMessages();
        TreeViewItemViewModel.Root.IsSelected = true;
    }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<TreeViewItemViewModel> Items { get; } = new();

    public ObservableCollection<TreeViewItemViewModel> Roots { get; } = new();

    const string _ready = "准备就绪";

    readonly IMessenger _messenger;
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
                if (itemViewModel == TreeViewItemViewModel.Root)
                    ListDrives(itemViewModel);
                else
                {
                    StartProcess(itemViewModel.FullPath);
                    return;
                }
            }
            else
                await ListItemsInSelectedDirectoryAsync(info);
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

    void RegisterMessages()
    {
        _messenger.Register(this, "Expanding", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanding(m));
        _messenger.Register(this, "Expanded", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanded());
        _messenger.Register(this, "Selected", async (MainWindowViewModel r, TreeViewItemViewModel m) => await r.OnItemSelectedAsync(m));
    }

    void StartProcess(string path)
    {
        var isExe = Path.GetExtension(path).ToLower() == ".exe";
        StatusInfo = isExe ? $"正在启动：{path}" : $"正在打开：{path}";
        if (isExe)
            new ToastContentBuilder()
            .AddText("正在尝试启动")
            .AddText(path)
            .Show();
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.ErrorDialog = true;
        if (process.Start())
        {
            _snackbarService.Show("提示", $"已成功打开或运行：{path}", SymbolRegular.Checkmark24, ControlAppearance.Success);
            if (isExe)
                new ToastContentBuilder()
                .AddText("已启动")
                .AddText(path)
                .Show();
        }
    }

    void ListDrives(TreeViewItemViewModel itemViewModel)
    {
        Items.Clear();
        foreach (var item in itemViewModel.Items)
            Items.Add(item);
    }

    async Task ListItemsInSelectedDirectoryAsync(DirectoryInfo info)
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

}
