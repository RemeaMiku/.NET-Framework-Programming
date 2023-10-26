using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentManagemantSystem.Models;

namespace StudentManagemantSystem.Data;

public class StudentManagementDbContext : DbContext
{
    public DbSet<School> SchoolTable { get; set; } = null!;

    public DbSet<Class> ClassTable { get; set; } = null!;

    public DbSet<Student> StudentTable { get; set; } = null!;

    public DbSet<Log> Logs { get; set; } = null!;

    public string DbPath { get; init; }

    public StudentManagementDbContext()
    {
        DbPath = "BlueArchive.db";
    }

    public StudentManagementDbContext(string dbPath)
    {
        DbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");

}
