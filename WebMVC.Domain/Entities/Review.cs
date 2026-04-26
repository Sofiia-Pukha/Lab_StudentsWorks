using System.ComponentModel.DataAnnotations.Schema;

namespace WebMVC.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }

    public int StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User Student { get; set; }

    public int TeacherId { get; set; }
    
    [ForeignKey("TeacherId")]
    [InverseProperty("Reviews")] 
    public Teacher Teacher { get; set; }

    public int WorkId { get; set; }
    [ForeignKey("WorkId")]
    public Work Work { get; set; }
}