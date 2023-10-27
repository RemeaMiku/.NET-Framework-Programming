using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
        _dbContext = new StudentManagementDbContext(Path.Combine(Environment.CurrentDirectory, "BlueArchive.db"));
        _dbContext.StudentTable.Load();
        _dbContext.SchoolTable.Load();
        _dbContext.ClassTable.Load();
        _dbContext.LogTable.Load();
        _dbContext.ChangeTracker.StateChanged += OnChangeTrackerStateChanged;
        LogCollection = _dbContext.LogTable.Local.ToObservableCollection();
        Refresh();
    }

    #endregion Public Constructors

    #region Public Properties

    public ISnackbarService SnackbarService { get; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<Log>? LogCollection { get; private set; }

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
                _displayPropertyNameDic.Clear();
                DisplayNames.Clear();
                var type = Type.GetType($"StudentManagemantSystem.Models.{EntityTypeName!}");
                var properties = type?.GetProperties();
                foreach (var property in properties!)
                {
                    var propertyName = property.Name;
                    var displayName = (property.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute)?.DisplayName;
                    if (propertyName is not null && displayName is not null)
                    {
                        _displayPropertyNameDic.Add(displayName, propertyName);
                        DisplayNames.Add(displayName);
                    }
                }
                SelectedDisplayName = DisplayNames[0];
            }
        }
    }

    #endregion Public Properties

    #region Public Methods

    [RelayCommand]
    public void Refresh()
    {
        Keyword = string.Empty;
        StudentCollection = _dbContext.StudentTable.Local.ToObservableCollection();
        ClassCollection = _dbContext.ClassTable.Local.ToObservableCollection();
        SchoolCollection = _dbContext.SchoolTable.Local.ToObservableCollection();
    }

    #endregion Public Methods

    #region Private Fields

    readonly StudentManagementDbContext _dbContext;

    readonly Dictionary<string, string> _displayPropertyNameDic = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;
    [ObservableProperty]
    ObservableCollection<Student>? _studentCollection;

    [ObservableProperty]
    ObservableCollection<Class>? _classCollection;

    [ObservableProperty]
    ObservableCollection<School>? _schoolCollection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    IList? _selectedItems;

    string _entityTypeName = string.Empty;

    [ObservableProperty]
    string _selectedDisplayName = string.Empty;

    [ObservableProperty]
    string _keyword = string.Empty;

    #endregion Private Fields

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

    private void OnChangeTrackerStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        if (e.Entry.Entity is Log)
            return;
        var entry = e.Entry;
        var entityId = (int)entry.Property("Id").CurrentValue!;
        var entityTypeName = entry.Entity.GetType().Name;
        var log = new Log() { EntityId = entityId, EntityTypeName = entityTypeName };
        if (e.OldState == EntityState.Added)
            log.OperationType = "接受添加";
        else if (e.NewState == EntityState.Modified)
            log.OperationType = "尝试修改";
        else if (e.NewState == EntityState.Unchanged)
            log.OperationType = "接受修改";
        else if (e.NewState == EntityState.Deleted)
            log.OperationType = "尝试删除";
        else if (e.NewState == EntityState.Detached)
            log.OperationType = "接受删除";
        log.Description = GetAllPropertyInfosOfEntry(entry);
        _dbContext.LogTable.Add(log);
    }
    [RelayCommand]
    void Add()
    {
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
    [RelayCommand]
    async Task Delete()
    {
        if (!HasSelection)
            return;
        try
        {
            IsBusy = true;
            await App.Current.Dispatcher.InvokeAsync(() =>
               {
                   for (int i = SelectedItems!.Count - 1; i >= 0; i--)
                   {
                       var item = SelectedItems[i];
                       if (item is Student student)
                       {
                           _dbContext.StudentTable.Local.Remove(student);
                           StudentCollection!.Remove(student);
                       }
                       else if (item is Class @class)
                       {
                           _dbContext.ClassTable.Local.Remove(@class);
                           ClassCollection!.Remove(@class);
                       }
                       else if (item is School school)
                       {
                           _dbContext.SchoolTable.Local.Remove(school);
                           SchoolCollection!.Remove(school);
                       }
                   }
               });
        }
        catch (Exception ex)
        {
            SnackbarService.Show("程序异常", ex.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task Save()
    {
        if (!_dbContext.ChangeTracker.HasChanges())
            return;
        try
        {
            IsBusy = true;
            var status = await _dbContext.SaveChangesAsync();
            WeakReferenceMessenger.Default.Send("Saved");
            SnackbarService.Show("已成功保存", $"数据库路径：{_dbContext.DbPath}", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            SnackbarService.Show("保存失败", $"可能是约束冲突，请检查输入后重试：{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException?.Message}", SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task Search()
    {
        try
        {
            IsBusy = true;
            await Task.Run(() =>
            {
                var type = Type.GetType($"StudentManagemantSystem.Models.{EntityTypeName!}");
                var property = type!.GetProperty(_displayPropertyNameDic[SelectedDisplayName]);
                switch (EntityTypeName)
                {
                    case nameof(Student):
                        StudentCollection = new(_dbContext.StudentTable.Local.Where(s => property!.GetValue(s)!.ToString()!.Contains(Keyword)));
                        break;
                    case nameof(Class):
                        ClassCollection = new(_dbContext.ClassTable.Local.Where(c => property!.GetValue(c)!.ToString()!.Contains(Keyword)));
                        break;
                    case nameof(School):
                        SchoolCollection = new(_dbContext.SchoolTable.Local.Where(s => property!.GetValue(s)!.ToString()!.Contains(Keyword)));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
        }
        catch (Exception ex)
        {
            SnackbarService.Show("程序异常", ex.Message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Private Methods
}
