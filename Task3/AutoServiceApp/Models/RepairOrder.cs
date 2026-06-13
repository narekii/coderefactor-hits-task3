namespace AutoServiceApp.Models;

public class RepairOrder : BaseEntity
{
    public string OrderNumber { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string CarId { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public Customer? Customer { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Car? Car { get; set; }
    public string ProblemDescription { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public string AssignedMechanicId { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public Mechanic? AssignedMechanic { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public decimal Cost { get; set; }
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;
    public List<RepairWork> Works { get; private set; } = new();
    public List<string> UsedPartIds { get; private set; } = new();
    public List<string> StatusHistory { get; private set; } = new();
    public OrderType Type { get; set; } = OrderType.Standard;
    public bool NeedTaxi { get; set; }
    public string? WarrantyNumber { get; set; }
    public bool ApprovedByDealer { get; set; }

    public void AddWork(RepairWork work)
    {
        Works.Add(work);
    }

    public void AddUsedPart(string partId)
    {
        UsedPartIds.Add(partId);
    }

    public override string ToString()
    {
        var client = Customer?.Name ?? CustomerId;
        var car = Car == null ? CarId : $"{Car.Make} {Car.Model}";
        return $"{OrderNumber}: {client}, {car}, {Status}, {Cost:C}";
    }
    public void LogStatusChange(OrderStatus newStatus)
    {
        Status = newStatus;
        StatusHistory.Add($"{DateTime.Now:g}: status changed to {newStatus}");
    }
}
