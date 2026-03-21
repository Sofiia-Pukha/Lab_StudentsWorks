namespace WebMVC.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; }
    
    // Зовнішній ключ
    public int DepartmentId { get; set; }
    public Department Department { get; set; } // Зв'язок з кафедрою
}