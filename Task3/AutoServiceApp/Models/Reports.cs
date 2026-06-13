namespace AutoServiceApp.Models;

public abstract class BaseReport
{
    public string Title { get; set; } = "";
    public DateTime From { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime To { get; set; } = DateTime.Today;
    public string Text { get; set; } = "";
}

public class RepairReport : BaseReport
{
    public List<RepairOrder> Orders { get; set; } = new();
    public decimal ExtraRevenue { get; set; }
    public int ApprovedCount { get; set; }
}
