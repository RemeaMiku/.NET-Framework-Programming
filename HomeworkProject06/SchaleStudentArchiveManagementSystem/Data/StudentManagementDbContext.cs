using Microsoft.EntityFrameworkCore;
using SchaleStudentArchiveManagementSystem.Models;

namespace SchaleStudentArchiveManagementSystem.Data;

public class StudentManagementDbContext : DbContext
{
    #region Public Constructors

    public StudentManagementDbContext()
    {
        DbPath = "BlueArchive.db";
    }

    public StudentManagementDbContext(string dbPath)
    {
        DbPath = dbPath;
    }

    #endregion Public Constructors

    #region Public Properties

    public DbSet<School> SchoolTable { get; set; } = null!;

    public DbSet<Class> ClassTable { get; set; } = null!;

    public DbSet<Student> StudentTable { get; set; } = null!;

    public DbSet<Log> LogTable { get; set; } = null!;

    public string DbPath { get; init; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");

    #endregion Protected Methods

}
