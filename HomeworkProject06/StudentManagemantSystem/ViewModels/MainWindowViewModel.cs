﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StudentManagemantSystem.Data;
using StudentManagemantSystem.Models;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace StudentManagemantSystem.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{

    #region Public Constructors

    public MainWindowViewModel(ISnackbarService snackbarService)
    {
        SnackbarService = snackbarService;
    }

    #endregion Public Constructors

    #region Public Properties

    public bool IsConnected => _dbContext is not null;

    public bool IsNotSearching => !IsSearching;

    public bool IsLoggedIn => _dbContext is not null;

    public CollectionViewSource DataGridViewSource { get; } = new();

    public CollectionViewSource LogViewSource { get; } = new();

    public ISnackbarService SnackbarService { get; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<string> DisplayNames { get; } = new();

    public bool HasSelection => SelectedItems != null && SelectedItems.Count != 0;

    public string EntityTypeName
    {
        get => _entityTypeName;
        set
        {
            if (value != _entityTypeName)
            {
                _entityTypeName = value;
                Reset();
                UpdateSearchDisplayNames();
            }
        }
    }

    public string LogKeyWord
    {
        get => _logKeyWord;
        set
        {
            if (value != _logKeyWord)
            {
                _logKeyWord = value;
                FilterLogsCommand.Execute(null);
            }
        }
    }

    public IList? SelectedItems
    {
        get => _selectedItems;
        set
        {
            _selectedItems = value;
            OnPropertyChanged(nameof(HasSelection));
            if (_selectedItems is not null && _selectedItems.Count != 0)
                InfoMessage = $"已选中 {_selectedItems.Count} 项";
            else
                InfoMessage = IsSearching ? SearchingInfoMessage : DefaultInfoMessage;
        }
    }

    #endregion Public Properties

    #region Private Fields

    readonly Dictionary<string, string> _displayPropertyNameDic = new();

    [ObservableProperty]
    string _dbPath = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotSearching))]
    bool _isSearching = false;

    string _logKeyWord = string.Empty;

    StudentManagementDbContext? _dbContext;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    IList? _selectedItems;

    [ObservableProperty]
    string _infoMessage = "未连接数据库";

    string _entityTypeName = nameof(Student);

    [ObservableProperty]
    string _selectedDisplayName = string.Empty;

    [ObservableProperty]
    string _keyword = string.Empty;

    #endregion Private Fields

    #region Private Properties

    string? DatabaseFileName => Path.GetFileName(_dbContext?.DbPath);

    int DataGridItemsCount
        => IsSearching ? DataGridViewSource.View.Cast<object>().Count() : DataGridViewSource.View.SourceCollection.Cast<object>().Count();

    string DefaultInfoMessage
        => $"已展示 {DatabaseFileName}.{EntityTypeName}Table 表结果，共 {DataGridItemsCount} 条记录";

    string SearchingInfoMessage
        => $"已展示 {DatabaseFileName}.{EntityTypeName}Table 表中属性 {SelectedDisplayName} 包含 {Keyword} 的结果，共 {DataGridItemsCount} 条记录";

    #endregion Private Properties

    #region Private Methods

    static string GetAllPropertyInfosOfEntry(EntityEntry entry)
    {
        var stringBuilder = new StringBuilder();
        foreach (var property in entry.Properties)
        {
            var displayName = property.Metadata.PropertyInfo!.GetCustomAttribute<DisplayNameAttribute>()!.DisplayName;
            if (displayName.Contains("集合"))
                continue;
            stringBuilder.Append(displayName);
            stringBuilder.Append('：');
            var isModified = property.OriginalValue!.ToString() != property.CurrentValue!.ToString();
            var value = isModified ? $"{property.OriginalValue} -> {property.CurrentValue}" : property.CurrentValue!.ToString();
            stringBuilder.Append(value);
            stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
    }

    static string? GetLogOperationType(EntityState oldState, EntityState newState)
    {
        if (oldState == EntityState.Added && newState == EntityState.Unchanged)
            return "接受添加";
        else if (oldState == EntityState.Unchanged && newState == EntityState.Modified)
            return "尝试修改";
        else if (oldState == EntityState.Modified && newState == EntityState.Unchanged)
            return "接受修改";
        else if (newState == EntityState.Deleted)
            return "尝试删除";
        else if (oldState == EntityState.Deleted && newState == EntityState.Detached)
            return "接受删除";
        else
            return default;
    }

    [RelayCommand]
    async Task ConnectToDatabase()
    {
        if (!Path.Exists(DbPath))
        {
            SnackbarService.Show("连接失败", $"{DbPath} 不存在", SymbolRegular.Dismiss24, ControlAppearance.Danger);
            return;
        }
        if (_dbContext is not null)
        {
            _dbContext.Dispose();
            _dbContext = null;
        }
        try
        {
            IsBusy = true;
            InfoMessage = $"正在加载：{DbPath}";
            await LoadDbContext();
            Reset();
            UpdateSearchDisplayNames();
            DataGridViewSource.Filter += OnDataGridViewSourceFilter;
        }
        catch (Exception ex)
        {
            SnackbarService.Show("连接出错：", ex.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
            InfoMessage = $"连接 {DbPath} 出错";
        }
        finally
        {
            IsBusy = false;
        }
    }
    async Task LoadDbContext()
    {
        _dbContext = new StudentManagementDbContext(DbPath);
        await _dbContext.StudentTable.LoadAsync();
        await _dbContext.SchoolTable.LoadAsync();
        await _dbContext.ClassTable.LoadAsync();
        await _dbContext.LogTable.LoadAsync();
        _dbContext.ChangeTracker.StateChanged += OnChangeTrackerStateChanged;
        LogViewSource.Source = _dbContext.LogTable.Local.ToObservableCollection();
    }
    private void OnDataGridViewSourceFilter(object sender, FilterEventArgs e)
    {
        if (!IsSearching)
        {
            e.Accepted = true;
            return;
        }
        e.Accepted = e.Item
            .GetType()!
            .GetProperty(_displayPropertyNameDic[SelectedDisplayName])!
            .GetValue(e.Item)!
            .ToString()!
            .Contains(Keyword);
    }
    [RelayCommand]
    async Task FilterLogs()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        IsBusy = true;
        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            if (string.IsNullOrWhiteSpace(LogKeyWord))
                LogViewSource.Source = _dbContext.LogTable.Local.ToObservableCollection();
            else
                LogViewSource.Source = _dbContext.LogTable.Local.Where(l => l.ToString().Contains(LogKeyWord));
        });
        LogViewSource.View.Refresh();
        IsBusy = false;
    }

    void UpdateSearchDisplayNames()
    {
        _displayPropertyNameDic.Clear();
        DisplayNames.Clear();
        var type = Type.GetType($"StudentManagemantSystem.Models.{EntityTypeName!}");
        var properties = type?.GetProperties();
        foreach (var property in properties!)
        {
            var propertyName = property.Name;
            var displayName = (property.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute)?.DisplayName;
            if (propertyName is not null && displayName is not null && !displayName.Contains("集合"))
            {
                _displayPropertyNameDic.Add(displayName, propertyName);
                DisplayNames.Add(displayName);
            }
        }
        SelectedDisplayName = DisplayNames[0];
    }

    [RelayCommand]
    void Reset()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        Keyword = string.Empty;
        DataGridViewSource.Source = EntityTypeName switch
        {
            nameof(Student) => _dbContext.StudentTable.Local.ToObservableCollection(),
            nameof(Class) => _dbContext.ClassTable.Local.ToObservableCollection(),
            nameof(School) => _dbContext.SchoolTable.Local.ToObservableCollection(),
            _ => throw new NotImplementedException(),
        };
        DataGridViewSource.View.Refresh();
        IsSearching = false;
        InfoMessage = DefaultInfoMessage;
    }
    private void OnChangeTrackerStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        if (e.Entry.Entity is Log)
            return;
        var operationType = GetLogOperationType(e.OldState, e.NewState);
        if (operationType is null)
            return;
        var entry = e.Entry;
        var entityId = (int)entry.Property("Id").CurrentValue!;
        var entityTypeName = entry.Entity.GetType().Name;
        var description = GetAllPropertyInfosOfEntry(entry);
        var log = new Log()
        {
            EntityId = entityId,
            EntityTypeName = entityTypeName,
            OperationType = operationType,
            Description = description
        };
        _dbContext.LogTable.Add(log);
    }
    [RelayCommand]
    void Add()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        switch (EntityTypeName)
        {
            case nameof(Student):
                _dbContext.StudentTable.Local.Add(new());
                break;
            case nameof(Class):
                _dbContext.ClassTable.Local.Add(new());
                break;
            case nameof(School):
                _dbContext.SchoolTable.Local.Add(new());
                break;
            default:
                throw new NotImplementedException();
        }
    }

    void RemoveStudent(Student student) => _dbContext!.StudentTable.Remove(student);

    void RemoveClass(Class @class)
    {
        if (@class.Students is not null && @class.Students.Any())
        {
            SnackbarService.Show("删除被阻止", $"有学生属于 Id：{@class.Id}, Name：{@class.Name} 的班级。请先删除其学生或将学生转移至其他班级。", SymbolRegular.DeleteDismiss24, ControlAppearance.Caution);
            return;
        }
        _dbContext!.ClassTable.Remove(@class);
    }

    void RemoveSchool(School school)
    {
        if (school.Classes is not null && school.Classes.Any())
        {
            SnackbarService.Show("删除被阻止", $"有班级属于 Id：{school.Id}, Name：{school.Name} 的学校。请先删除其班级或将班级转移至其他学校。", SymbolRegular.DeleteDismiss24, ControlAppearance.Caution);
            return;
        }
        _dbContext!.SchoolTable.Remove(school);
    }

    async Task RemoveSelectedItems()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            for (int i = SelectedItems!.Count - 1; i >= 0; i--)
            {
                var item = SelectedItems[i];
                if (item is Student student)
                    RemoveStudent(student);
                else if (item is Class @class)
                    RemoveClass(@class);
                else if (item is School school)
                    RemoveSchool(school);
            }
        });
    }

    [RelayCommand]
    async Task Delete()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        if (!HasSelection)
            return;
        try
        {
            IsBusy = true;
            InfoMessage = $"正在尝试删除 {SelectedItems!.Count} 项结果";
            await RemoveSelectedItems();
            DataGridViewSource.View.Refresh();
        }
        catch (Exception ex)
        {
            SnackbarService.Show("程序异常", ex.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            InfoMessage = IsSearching ? SearchingInfoMessage : DefaultInfoMessage;
        }
    }

    [RelayCommand]
    async Task Save()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Not connected to the database.");
        try
        {
            if (!_dbContext.ChangeTracker.HasChanges())
            {
                InfoMessage = "未找到更改";
                await Task.Delay(3000);
                return;
            }
            IsBusy = true;
            InfoMessage = "正在尝试保存更改";
            var status = await _dbContext.SaveChangesAsync();
            DataGridViewSource.View.Refresh();
            SnackbarService.Show("已成功保存", $"数据库路径：{_dbContext.DbPath}", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            SnackbarService.Show("保存失败", $"可能是约束冲突，请检查输入后重试：{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException?.Message}", SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            InfoMessage = IsSearching ? SearchingInfoMessage : DefaultInfoMessage;
        }
    }

    [RelayCommand]
    async Task Search()
    {
        try
        {
            IsBusy = true;
            IsSearching = true;
            InfoMessage = "正在搜索";
            await App.Current.Dispatcher.InvokeAsync(DataGridViewSource.View.Refresh);
        }
        catch (Exception ex)
        {
            SnackbarService.Show("程序异常", ex.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
            InfoMessage = SearchingInfoMessage;
        }
    }

    #endregion Private Methods
}
