using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Extensions;
using Wpf.Ui.Common;

namespace Explorer.ViewModels;

public partial class TreeViewItemViewModel : ObservableObject
{

    #region Public Constructors

    public TreeViewItemViewModel(DriveInfo info)
    {
        Icon = SymbolRegular.HardDrive20;
        Text = info.Name;
        FullPath = info.Name;
        LoadItems(info.RootDirectory.EnumerateAccessibleDirectories());
    }

    public TreeViewItemViewModel(FileSystemInfo info)
    {
        if (info is FileInfo fileInfo)
        {
            Icon = SymbolRegular.Document24;
            Text = fileInfo.Name;
            FullPath = fileInfo.FullName;
        }
        else if (info is DirectoryInfo directoryInfo)
        {
            Icon = SymbolRegular.Folder24;
            Text = directoryInfo.Name;
            FullPath = directoryInfo.FullName;
            if (directoryInfo.HasAccessibleSubdirectory())
                Items.Add(Empty);
        }
        else
            throw new NotImplementedException();
    }

    #endregion Public Constructors

    #region Public Properties

    public static TreeViewItemViewModel Empty { get; } = new();
    public static TreeViewItemViewModel Root { get; } = new(SymbolRegular.Desktop24, Environment.MachineName, Environment.MachineName);
    public SymbolRegular Icon { get; }
    public string Text { get; } = string.Empty;
    public string FullPath { get; } = string.Empty;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                OnPropertyChanging(nameof(IsSelected));
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                if (value)
                    WeakReferenceMessenger.Default.Send(this, "Selected");
            }
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (value != _isExpanded)
            {
                OnPropertyChanging(nameof(IsExpanded));
                _isExpanded = value;
                if (value && Items.Any() && ReferenceEquals(Items[0], Empty))
                {
                    WeakReferenceMessenger.Default.Send(this, "Expanding");
                    Items.Clear();
                    LoadItems(GetDirectoryInfo()!.EnumerateAccessibleDirectories());
                    WeakReferenceMessenger.Default.Send(this, "Expanded");
                }
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }

    public ObservableCollection<TreeViewItemViewModel> Items { get; } = new();

    #endregion Public Properties

    #region Public Methods

    public DirectoryInfo? GetDirectoryInfo() => Directory.Exists(FullPath) ? new(FullPath) : null;

    #endregion Public Methods

    #region Private Fields

    bool _isExpanded = false;

    bool _isSelected = false;

    #endregion Private Fields

    #region Private Constructors

    private TreeViewItemViewModel()
    {
    }
    private TreeViewItemViewModel(SymbolRegular icon, string text, string path)
    {
        Icon = icon;
        Text = text;
        FullPath = path;
    }

    #endregion Private Constructors

    #region Private Methods

    void LoadItems(IEnumerable<DirectoryInfo> infos)
    {
        foreach (var info in infos)
            Items.Add(new(info));
    }

    [RelayCommand]
    void Select() => WeakReferenceMessenger.Default.Send(this, "Selected");

    #endregion Private Methods

}
