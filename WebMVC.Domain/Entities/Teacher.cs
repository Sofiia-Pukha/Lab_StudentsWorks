using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Domain.Entities;

namespace WebMVC.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    
    public int DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;

    public ICollection<Work> Works { get; set; } = new List<Work>();
    public List<Review> Reviews { get; set; } = new List<Review>();
}