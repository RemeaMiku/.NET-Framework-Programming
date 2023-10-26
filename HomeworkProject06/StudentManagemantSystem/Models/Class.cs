// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System.Collections.Generic;
using System.ComponentModel;

namespace StudentManagemantSystem.Models;

public class Class
{
    #region Public Properties

    [DisplayName("班级Id")]
    public int Id { get; set; }

    [DisplayName("班级名称")]
    public string Name { get; set; } = null!;

    [DisplayName("年级")]
    public int Grade { get; set; }

    [DisplayName("所属学校Id")]
    public int SchoolId { get; set; }

    [DisplayName("所属学校名称")]
    public School School { get; set; } = null!;

    [DisplayName("学生集合")]
    public ICollection<Student> Students { get; set; } = null!;

    [DisplayName("描述")]
    public string? Description { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override string ToString() => Name;

    #endregion Public Methods
}