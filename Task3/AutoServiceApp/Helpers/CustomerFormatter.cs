using AutoServiceApp.Models;

namespace AutoServiceApp.Helpers;

public class CustomerFormatter
{
    public string Format(Customer customer) => $"{customer.Name} / {customer.Phone}";
}
