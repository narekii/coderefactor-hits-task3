namespace AutoServiceApp.Models;

public class RepairWork : BaseEntity
{
    public string Name { get; set; } = "";
    public double Hours { get; set; }
    public decimal Cost { get; set; }

    public override string ToString() => $"{Name} - {Hours} h - {Cost:C}";
}
