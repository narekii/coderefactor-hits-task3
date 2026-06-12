namespace AutoServiceApp.Models;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; }
}
