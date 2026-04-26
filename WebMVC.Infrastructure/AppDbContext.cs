using WebMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebMVC.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

        public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Work> Works { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<AdminLog> AdminLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.Department)
            .WithMany()
            .HasForeignKey(t => t.DepartmentId);

        modelBuilder.Entity<Work>()
            .HasOne(w => w.Category)
            .WithMany()
            .HasForeignKey(w => w.CategoryId);

        modelBuilder.Entity<Work>()
            .HasOne(w => w.Student)
            .WithMany()
            .HasForeignKey(w => w.StudentId);

        modelBuilder.Entity<Work>()
            .HasOne(w => w.Teacher)
            .WithMany()
            .HasForeignKey(w => w.TeacherId);

        modelBuilder.Entity<Review>().HasOne(r => r.Student).WithMany().HasForeignKey(r => r.StudentId);
        modelBuilder.Entity<Review>().HasOne(r => r.Teacher).WithMany().HasForeignKey(r => r.TeacherId);
        modelBuilder.Entity<Review>().HasOne(r => r.Work).WithMany().HasForeignKey(r => r.WorkId);

        modelBuilder.Entity<AdminLog>().HasOne(a => a.Admin).WithMany().HasForeignKey(a => a.AdminId);
    }
}

