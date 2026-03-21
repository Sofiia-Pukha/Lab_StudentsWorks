namespace WebMVC.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }

    // Зовнішні ключі
    public int StudentId { get; set; }
    public User Student { get; set; }

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; }

    public int WorkId { get; set; }
    public Work Work { get; set; }
}