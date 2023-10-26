using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagemantSystem.Models;

public class Log
{
    public int Id { get; set; }

    public EntityType EntityType { get; set; }

    public int EntityId { get; init; }

    public DateTime Time { get; init; } = DateTime.Now;

    public OperationType OperationType { get; init; }

    public string? Description { get; set; }
}

public enum EntityType
{
    Student,
    Class,
    School
}

public enum OperationType
{
    Add,
    Delete,
    Edit,
    Select
}
