using System.ComponentModel.DataAnnotations.Schema;

namespace WebMVC.Domain.Entities;

public class Work
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? FileUrl { get; set; }
    
    public int? Grade { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int ExecutionYear { get; set; }

    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;

    public int StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User Student { get; set; } = null!;

    public int TeacherId { get; set; }
    [ForeignKey("TeacherId")]
    public Teacher Teacher { get; set; } = null!;
}