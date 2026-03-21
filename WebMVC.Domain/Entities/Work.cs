using System;

namespace WebMVC.Domain.Entities;

public class Work
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FileUrl { get; set; }
    public int? Grade { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ExecutionYear { get; set; }

    // Зовнішні ключі
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public int StudentId { get; set; }
    public User Student { get; set; }

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; }
}