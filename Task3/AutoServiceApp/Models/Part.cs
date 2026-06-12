namespace AutoServiceApp.Models;

public class Part : BaseEntity
{
    public string Name { get; set; } = "";
    public string Article { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public override string ToString() => $"{Name} [{Article}], {Price:C}, stock {Stock}";
}
