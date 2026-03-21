using WebMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebMVC.Infrastructure;

public class AppDbContext : DbContext
{
    // Цей конструктор потрібен, щоб підключитися до бази
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    

    // Реєструємо наші таблиці (сутності)
    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Work> Works { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<AdminLog> AdminLogs { get; set; }

    // Тут ми налаштовуємо зв'язки (Fluent API) - ті самі стрілочки з твоєї діаграми
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Вчитель належить до Кафедри
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.Department)
            .WithMany()
            .HasForeignKey(t => t.DepartmentId);

        // Робота має Категорію
        modelBuilder.Entity<Work>()
            .HasOne(w => w.Category)
            .WithMany()
            .HasForeignKey(w => w.CategoryId);

        // Роботу додав Студент
        modelBuilder.Entity<Work>()
            .HasOne(w => w.Student)
            .WithMany()
            .HasForeignKey(w => w.StudentId);

        // Роботу перевіряє Вчитель
        modelBuilder.Entity<Work>()
            .HasOne(w => w.Teacher)
            .WithMany()
            .HasForeignKey(w => w.TeacherId);

        // Відгуки
        modelBuilder.Entity<Review>().HasOne(r => r.Student).WithMany().HasForeignKey(r => r.StudentId);
        modelBuilder.Entity<Review>().HasOne(r => r.Teacher).WithMany().HasForeignKey(r => r.TeacherId);
        modelBuilder.Entity<Review>().HasOne(r => r.Work).WithMany().HasForeignKey(r => r.WorkId);

        // Логи адміна
        modelBuilder.Entity<AdminLog>().HasOne(a => a.Admin).WithMany().HasForeignKey(a => a.AdminId);
    }
}

