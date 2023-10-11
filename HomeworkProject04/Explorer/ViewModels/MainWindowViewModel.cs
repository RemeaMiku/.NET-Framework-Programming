using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Extensions;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    readonly ISnackbarService _snackbarService;
    public MainWindowViewModel(ISnackbarService snackbarService)
    {
        _snackbarService = snackbarService;
        var driveInfos = DriveInfo.GetDrives();
        foreach (var driveInfo in driveInfos)
            TreeViewItemViewModel.Root.Items.Add(new(driveInfo));
        Roots.Add(TreeViewItemViewModel.Root);
        WeakReferenceMessenger.Default.Register(this, "Expanding", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanding(m));
        WeakReferenceMessenger.Default.Register(this, "Expanded", (MainWindowViewModel r, TreeViewItemViewModel m) => r.OnTreeViewItemExpanded());
        WeakReferenceMessenger.Default.Register(this, "Selected", async (MainWindowViewModel r, TreeViewItemViewModel m) => await r.OnSelectedItemChanged(m));
    }
    [ObservableProperty]
    string _currentPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy;

    const string _ready = "准备就绪";

    [ObservableProperty]
    string _statusInfo = _ready;

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<TreeViewItemViewModel> Items { get; } = new();

    public ObservableCollection<TreeViewItemViewModel> Roots { get; } = new();

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

    async Task OnSelectedItemChanged(TreeViewItemViewModel itemViewModel)
    {
        try
        {
            IsBusy = true;
            StatusInfo = $"正在加载：{itemViewModel.FullPath}";
            var info = itemViewModel.GetDirectoryInfo();
            if (info is null)
            {
                if (itemViewModel == TreeViewItemViewModel.Root)
                    LoadItems(itemViewModel);
                else
                    RunProcess(itemViewModel.FullPath);
            }
            else
                await LoadDirectory(info);
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
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.ErrorDialog = true;
        if (process.Start())
            _snackbarService.Show("提示", $"已打开文件：{path}。", SymbolRegular.Check24, ControlAppearance.Success);
    }

    void LoadItems(TreeViewItemViewModel itemViewModel)
    {
        Items.Clear();
        CurrentPath = itemViewModel.FullPath;
        foreach (var item in itemViewModel.Items)
            Items.Add(item);
    }

    async Task LoadDirectory(DirectoryInfo info)
    {
        Items.Clear();
        CurrentPath = info.FullName;
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
}
