namespace AutoServiceApp.Models;

public enum OrderStatus
{
    New,
    Diagnostics,
    InProgress,
    WaitingForParts,
    Ready,
    Released
}

public enum PaymentType
{
    Cash,
    Card,
    Transfer
}
public enum OrderType
{
    Standard,
    Urgent,
    Warranty
}
