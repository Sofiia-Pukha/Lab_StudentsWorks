using System;

namespace WebMVC.Domain.Entities;

public class AdminLog
{
    public int Id { get; set; }
    public string TargetTable { get; set; }
    public string TargetColumn { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime ActionTime { get; set; } = DateTime.UtcNow;

    public int AdminId { get; set; }
    public User Admin { get; set; }
}