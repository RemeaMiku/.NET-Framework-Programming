using System;

namespace SchaleStudentArchiveManagementSystem.Models;

public class Log
{
    #region Public Properties

    public int Id { get; set; }

    public string EntityTypeName { get; set; } = null!;

    public int EntityId { get; set; }

    public DateTime Time { get; init; } = DateTime.Now;

    public string OperationType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    #endregion Public Properties

    #region Public Methods

    public override string ToString()
       => $"{Time:yyyy-MM-dd HH:mm:ss.ff}   {EntityTypeName} {EntityId} {OperationType} ：{Environment.NewLine}{Description}";

    #endregion Public Methods
}