using AutoServiceApp.Models;

namespace AutoServiceApp.Helpers;

public class OrderStatusHelper
{
    public List<string> CommonStatuses { get; set; } = new() { "New", "Diagnostics", "In Progress", "Waiting for Parts", "Ready", "Released" };

    public void MarkStatus(RepairOrder order, string status)
    {
        order.Status = status;
        order.StatusHistory.Add($"{DateTime.Now:g}: status changed to {status}");
        if (status == "Ready")
            order.CompletedAt = DateTime.Now;
    }
}
