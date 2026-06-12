namespace AutoServiceApp.Models;

public class Mechanic : BaseEntity
{
    public string Name { get; set; } = "";
    public string Specialization { get; set; } = "";
    public decimal HourRate { get; set; }
    public List<string> AssignedOrderIds { get; set; } = new();

    public override string ToString() => $"{Name} - {Specialization}, {HourRate:C}/h";
}
