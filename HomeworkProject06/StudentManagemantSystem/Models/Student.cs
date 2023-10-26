using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagemantSystem.Models;

public class Student
{
    [DisplayName("学生Id")]
    public int Id { get; set; }
    [DisplayName("学生姓名")]
    public string Name { get; set; } = null!;
    [DisplayName("性别")]
    public Gender Gender { get; set; }
    [DisplayName("所属班级Id")]
    public int ClassId { get; set; }
    [DisplayName("所属班级名称")]
    public Class Class { get; set; } = null!;
    [DisplayName("描述")]
    public string? Description { get; set; }
}

public enum Gender
{
    Male,
    Female
}