namespace AutoServiceApp.Models;

public class Customer : BaseEntity
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Car> Cars { get; private set; } = new();
    public PaymentType LastPaymentMethod { get; set; } = PaymentType.Cash;

    public string Export() => $"{Name};{Phone};{Email};{Address}";
    public override string ToString() => string.IsNullOrWhiteSpace(Phone) ? Name : $"{Name} ({Phone})";
}