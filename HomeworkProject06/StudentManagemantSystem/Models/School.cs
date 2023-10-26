using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagemantSystem.Models;

public class School
{
    [DisplayName("学校Id")]
    public int Id { get; set; }
    [DisplayName("学校名称")]
    public string Name { get; set; } = null!;
    [DisplayName("班级集合")]
    public ICollection<Class> Classes { get; set; } = null!;
    [DisplayName("描述")]
    public string? Description { get; set; }

    public override string ToString() => Name;
}
