using System.Collections.Generic;
using System.ComponentModel;

namespace SchaleStudentArchiveManagementSystem.Models;

public class School
{
    #region Public Properties

    [DisplayName("学校Id")]
    public int Id { get; set; }
    [DisplayName("学校名称")]
    public string Name { get; set; } = null!;
    [DisplayName("班级集合")]
    public ICollection<Class> Classes { get; set; } = null!;
    [DisplayName("描述")]
    public string Description { get; set; } = string.Empty;

    #endregion Public Properties

    #region Public Methods

    public override string ToString() => Name;

    #endregion Public Methods
}
