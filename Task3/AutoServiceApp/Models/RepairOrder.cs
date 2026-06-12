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
    public string Status { get; set; } = "New";
    public string AssignedMechanicId { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public Mechanic? AssignedMechanic { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public decimal Cost { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public List<RepairWork> Works { get; set; } = new();
    public List<string> UsedPartIds { get; set; } = new();
    public List<string> StatusHistory { get; set; } = new();

    public override string ToString()
    {
        var client = Customer?.Name ?? CustomerId;
        var car = Car == null ? CarId : $"{Car.Make} {Car.Model}";
        return $"{OrderNumber}: {client}, {car}, {Status}, {Cost:C}";
    }
}

public class UrgentRepairOrder : RepairOrder
{
    public bool NeedTaxi { get; set; }
    public decimal UrgentFee { get; set; } = 500;
}

public class WarrantyRepairOrder : RepairOrder
{
    public string WarrantyNumber { get; set; } = "";
    public bool ApprovedByDealer { get; set; }
}
