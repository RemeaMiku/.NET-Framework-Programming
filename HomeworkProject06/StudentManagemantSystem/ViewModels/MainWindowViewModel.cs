using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using StudentManagemantSystem.Data;
using StudentManagemantSystem.Models;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Interfaces;

namespace StudentManagemantSystem.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ISnackbarService SnackbarService { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    public bool IsNotBusy => !IsBusy;

    readonly StudentManagementDbContext _dbContext;

    public MainWindowViewModel(ISnackbarService snackbarService)
    {
        SnackbarService = snackbarService;
        _dbContext = new StudentManagementDbContext(Path.Combine(Environment.CurrentDirectory, "BlueArchive.db"));
        //_dbContext.Schools.Add(new School() { Name = "阿拜多斯高中" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "对策委员会" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "砂狼白子" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "小鸟游星野" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "黑见茜香" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "奥空绫音" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "十六夜野乃美" });
        //_dbContext.Schools.Add(new School() { Name = "山海经高级中学" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "玄龙门" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "近卫弥奈" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "玄武商会" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "朱城瑠美" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "梅花园" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "春原心菜" });
        //_dbContext.Schools.Add(new School() { Name = "百鬼夜行联合学园" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "阴阳部" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "和乐知世" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "桑上嘉穗" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "修行部" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "春日椿" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "水羽三森" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "勇美枫" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "忍术研究部" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "久田伊树菜" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "千鸟三千留" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "大野月夜" });
        //_dbContext.Schools.Add(new School() { Name = "三一综合学园" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "茶会" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "桐藤渚" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "圣园弥香" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "三一自卫队" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "守月铃美" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "宇泽玲纱" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "补课部" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "阿慈谷日富美" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "白洲梓" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "浦和花子" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "下江小春" });
        //_dbContext.Schools.Add(new School() { Name = "格黑娜学园" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "风纪委员会" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "空崎阳奈" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "天雨亚子" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "银镜伊织" });
        //_dbContext.Schools.Last().Classes.Last().Students.Add(new Student() { Name = "火宫千夏" });
        //_dbContext.Schools.Last().Classes.Add(new Class() { Name = "便利屋68" });

        //_dbContext.Schools.Add(new School() { Name = "千年科学学园" });
        //_dbContext.Schools.Add(new School() { Name = "赤冬联邦学园" });
        //_dbContext.Schools.Add(new School() { Name = "阿里乌斯分校" });
        //_dbContext.Schools.Add(new School() { Name = "瓦尔基里警察学院" });
        //_dbContext.Schools.Add(new School() { Name = "SRT特殊学园" });
        //_dbContext.SaveChanges();
        _dbContext.StudentTable.Load();
        _dbContext.SchoolTable.Load();
        _dbContext.ClassTable.Load();
        StudentCollection = _dbContext.StudentTable.Local.ToObservableCollection();
        ClassCollection = _dbContext.ClassTable.Local.ToObservableCollection();
        SchoolCollection = _dbContext.SchoolTable.Local.ToObservableCollection();
    }

    [ObservableProperty]
    ObservableCollection<Student>? _studentCollection;

    [ObservableProperty]
    ObservableCollection<Class>? _classCollection;

    [ObservableProperty]
    ObservableCollection<School>? _schoolCollection;


    readonly Dictionary<string, string> _displayPropertyNameDic = new();

    public ObservableCollection<string> DisplayNames { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    IList? _selectedItems;

    public bool HasSelection => SelectedItems != null && SelectedItems.Count != 0;

    //public List<Student> StudentSelectedCollection { get; } = new();
    //public List<School> SchoolSelectedCollection { get; } = new();
    //public List<Class> ClassSelectedCollection { get; } = new();


    string _entityTypeName = string.Empty;

    [ObservableProperty]
    string _selectedDisplayName = string.Empty;

    [ObservableProperty]
    string _keyword = string.Empty;

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

    [RelayCommand]
    void Add()
    {
        switch (EntityTypeName)
        {
            case nameof(Student):
                StudentCollection?.Add(new());
                break;
            case nameof(Class):
                ClassCollection?.Add(new());
                break;
            case nameof(School):
                SchoolCollection?.Add(new());
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
                           StudentCollection!.Remove(student);
                       else if (item is Class @class)
                           ClassCollection!.Remove(@class);
                       else if (item is School school)
                           SchoolCollection!.Remove(school);
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
        try
        {
            IsBusy = true;
            await _dbContext.SaveChangesAsync();
            SnackbarService.Show("已成功保存", $"数据库路径：{_dbContext.DbPath}", SymbolRegular.Checkmark24, ControlAppearance.Success);
            WeakReferenceMessenger.Default.Send("Saved");
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
                        StudentCollection = new(StudentCollection!.Where(s => property!.GetValue(s)!.ToString()!.Contains(Keyword)));
                        break;
                    case nameof(Class):
                        ClassCollection = new(ClassCollection!.Where(c => property!.GetValue(c)!.ToString()!.Contains(Keyword)));
                        break;
                    case nameof(School):
                        SchoolCollection = new(SchoolCollection!.Where(s => property!.GetValue(s)!.ToString()!.Contains(Keyword)));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
