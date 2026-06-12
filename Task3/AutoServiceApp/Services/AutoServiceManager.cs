using System.Text;
using AutoServiceApp.Helpers;
using AutoServiceApp.Models;
using AutoServiceApp.Storage;

namespace AutoServiceApp.Services;

public class AutoServiceManager
{
    public List<Customer> Customers { get; set; } = new();
    public List<Car> Cars { get; set; } = new();
    public List<RepairOrder> Orders { get; set; } = new();
    public List<Part> Parts { get; set; } = new();
    public List<Mechanic> Mechanics { get; set; } = new();
    public List<string> Notifications { get; set; } = new();

    public RepairOrder? _selectedOrder;
    public Part? _selectedPart;
    public decimal _tempDiscount;
    public BaseReport? _currentReport;

    public JsonFileStore<Customer> CustomerStore { get; set; } = new();
    public JsonFileStore<Car> CarStore { get; set; } = new();
    public JsonFileStore<RepairOrder> OrderStore { get; set; } = new();
    public JsonFileStore<Part> PartStore { get; set; } = new();
    public JsonFileStore<Mechanic> MechanicStore { get; set; } = new();
    public SmsNotifier SmsNotifier { get; set; } = new();
    public EmailSender EmailSender { get; set; } = new();
    public ReportService ReportService { get; set; } = new();
    public OrderStatusHelper StatusHelper { get; set; } = new();

    public void Load()
    {
        Customers = CustomerStore.Load("customers.json");
        Cars = CarStore.Load("cars.json");
        Orders = OrderStore.Load("orders.json");
        Parts = PartStore.Load("parts.json");
        Mechanics = MechanicStore.Load("mechanics.json");
        RelinkEverything();
        if (Customers.Count == 0 && Cars.Count == 0 && Mechanics.Count == 0)
            Seed();
    }

    public void SaveAll()
    {
        CustomerStore.Save("customers.json", Customers);
        CarStore.Save("cars.json", Cars);
        OrderStore.Save("orders.json", Orders);
        PartStore.Save("parts.json", Parts);
        MechanicStore.Save("mechanics.json", Mechanics);
    }

    public void RelinkEverything()
    {
        foreach (var c in Customers)
            c.Cars = Cars.Where(x => x.CustomerId == c.Id).ToList();

        foreach (var car in Cars)
            car.Owner = Customers.FirstOrDefault(x => x.Id == car.CustomerId);

        foreach (var order in Orders)
        {
            order.Customer = Customers.FirstOrDefault(x => x.Id == order.CustomerId);
            order.Car = Cars.FirstOrDefault(x => x.Id == order.CarId);
            order.AssignedMechanic = Mechanics.FirstOrDefault(x => x.Id == order.AssignedMechanicId);
        }

        foreach (var m in Mechanics)
            m.AssignedOrderIds = Orders.Where(x => x.AssignedMechanicId == m.Id).Select(x => x.Id).ToList();
    }

    public Customer AddCustomer(string name, string phone, string email, string address)
    {
        var c = new Customer { Name = name, Phone = phone, Email = email, Address = address };
        Customers.Add(c);
        SaveAll();
        return c;
    }

    public void UpdateCustomer(Customer customer, string name, string phone, string email, string address)
    {
        customer.Name = name;
        customer.Phone = phone;
        customer.Email = email;
        customer.Address = address;
        foreach (var order in Orders.Where(x => x.CustomerId == customer.Id))
            order.Customer = customer;
        SaveAll();
    }

    public void DeleteCustomer(Customer customer)
    {
        Customers.Remove(customer);
        foreach (var car in Cars.Where(x => x.CustomerId == customer.Id).ToList())
            Cars.Remove(car);
        foreach (var order in Orders.Where(x => x.CustomerId == customer.Id).ToList())
            Orders.Remove(order);
        SaveAll();
    }

    public Car AddCar(Customer? owner, string make, string model, int year, string vin, int mileage, string licensePlate)
    {
        var car = new Car
        {
            CustomerId = owner?.Id ?? "",
            Owner = owner,
            Make = make,
            Model = model,
            Year = year,
            Vin = vin,
            Mileage = mileage,
            LicensePlate = licensePlate
        };
        Cars.Add(car);
        if (owner != null)
            owner.Cars.Add(car);
        SaveAll();
        return car;
    }

    public void UpdateCar(Car car, Customer? owner, string make, string model, int year, string vin, int mileage, string licensePlate)
    {
        car.CustomerId = owner?.Id ?? "";
        car.Owner = owner;
        car.Make = make;
        car.Model = model;
        car.Year = year;
        car.Vin = vin;
        car.Mileage = mileage;
        car.LicensePlate = licensePlate;
        RelinkEverything();
        SaveAll();
    }

    public void DeleteCar(Car car)
    {
        Cars.Remove(car);
        foreach (var c in Customers)
            c.Cars.RemoveAll(x => x.Id == car.Id);
        foreach (var order in Orders.Where(x => x.CarId == car.Id).ToList())
            Orders.Remove(order);
        SaveAll();
    }

    public Mechanic AddMechanic(string name, string specialization, decimal hourRate)
    {
        var m = new Mechanic { Name = name, Specialization = specialization, HourRate = hourRate };
        Mechanics.Add(m);
        SaveAll();
        return m;
    }

    public void UpdateMechanic(Mechanic m, string name, string specialization, decimal hourRate)
    {
        m.Name = name;
        m.Specialization = specialization;
        m.HourRate = hourRate;
        SaveAll();
    }

    public void DeleteMechanic(Mechanic m)
    {
        Mechanics.Remove(m);
        foreach (var order in Orders.Where(o => o.AssignedMechanicId == m.Id))
        {
            order.AssignedMechanicId = "";
            order.AssignedMechanic = null;
        }
        SaveAll();
    }

    public Part AddPart(string name, string article, decimal price, int stock)
    {
        var p = new Part { Name = name, Article = article, Price = price, Stock = stock };
        Parts.Add(p);
        SaveAll();
        return p;
    }

    public void UpdatePart(Part part, string name, string article, decimal price, int stock)
    {
        part.Name = name;
        part.Article = article;
        part.Price = price;
        part.Stock = stock;
        SaveAll();
    }

    public void DeletePart(Part p)
    {
        Parts.Remove(p);
        SaveAll();
    }

    public RepairOrder CreateOrder(Customer? customer, Car? car, string description, Mechanic? mechanic, string status, string paymentMethod)
    {
        var order = new RepairOrder
        {
            OrderNumber = "RO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
            CustomerId = customer?.Id ?? "",
            CarId = car?.Id ?? "",
            Customer = customer,
            Car = car,
            ProblemDescription = description,
            AssignedMechanicId = mechanic?.Id ?? "",
            AssignedMechanic = mechanic,
            Status = status,
            PaymentMethod = paymentMethod,
            Cost = 0
        };
        order.StatusHistory.Add($"{DateTime.Now:g}: order created with status {status}");
        Orders.Add(order);
        if (mechanic != null)
            mechanic.AssignedOrderIds.Add(order.Id);
        SaveAll();
        return order;
    }

    public void UpdateOrder(RepairOrder order, Customer? customer, Car? car, string description, Mechanic? mechanic, string status, decimal cost, string paymentMethod)
    {
        order.CustomerId = customer?.Id ?? "";
        order.CarId = car?.Id ?? "";
        order.Customer = customer;
        order.Car = car;
        order.ProblemDescription = description;
        order.AssignedMechanicId = mechanic?.Id ?? "";
        order.AssignedMechanic = mechanic;
        order.PaymentMethod = paymentMethod;
        order.Cost = cost;
        if (order.Status != status)
            ChangeOrderStatus(order, status, "both");
        RelinkEverything();
        SaveAll();
    }

    public void ChangeOrderStatus(RepairOrder order, string newStatus, string notificationType)
    {
        _selectedOrder = order;
        StatusHelper.MarkStatus(order, newStatus);
        if (newStatus == "Ready")
            order.Cost = CalculateOrderCost(order, true, order.PaymentMethod);
        if (order.AssignedMechanic != null && !order.AssignedMechanic.AssignedOrderIds.Contains(order.Id))
            order.AssignedMechanic.AssignedOrderIds.Add(order.Id);
        NotifyAboutStatus(order, notificationType);
        SaveAll();
    }

    public void AddWorkToOrder(RepairOrder order, string name, double hours, decimal cost)
    {
        var work = new RepairWork { Name = name, Hours = hours, Cost = cost };
        order.Works.Add(work);
        order.Cost = CalculateOrderCost(order, false, order.PaymentMethod);
        SaveAll();
    }

    public bool UsePartForOrder(RepairOrder order, Part part, int qty)
    {
        _selectedPart = part;
        if (part.Stock < qty)
            return false;

        part.Stock -= qty;
        for (var i = 0; i < qty; i++)
            order.UsedPartIds.Add(part.Id);
        order.Cost += part.Price * qty * 1.50m;
        order.StatusHistory.Add($"{DateTime.Now:g}: part used {part.Name} x{qty}");
        SaveAll();
        return true;
    }

    public decimal CalculateOrderCost(RepairOrder order, bool final, string paymentMethod)
    {
        var works = order.Works.Sum(x => x.Cost + (decimal)x.Hours * (order.AssignedMechanic?.HourRate ?? 0));
        var parts = order.UsedPartIds.Select(id => Parts.FirstOrDefault(p => p.Id == id)).Where(p => p != null).Sum(p => p!.Price * 1.20m);
        var result = works + parts;
        if (paymentMethod == "card")
            result += result * 0.05m;
        if (order.Customer != null && order.Customer.Cars.Count > 2)
            result -= result * 0.10m;
        if (final && order.Status == "Ready")
            result += 500;
        if (result > 10000)
            _tempDiscount = result * 0.15m;
        else
            _tempDiscount = 0;
        return result - _tempDiscount;
    }

    public string BuildOrderDetails(RepairOrder order)
    {
        var sb = new StringBuilder();
        sb.AppendLine(order.ToString());
        sb.AppendLine(order.ProblemDescription);
        sb.AppendLine("Works:");
        foreach (var work in order.Works)
            sb.AppendLine(" - " + work);
        sb.AppendLine("History:");
        foreach (var h in order.StatusHistory)
            sb.AppendLine(" - " + h);
        if (order.Customer?.Cars.Count > 0)
            sb.AppendLine("First car owner phone: " + order.Customer.Cars[0].Owner?.Phone);
        return sb.ToString();
    }

    public string BuildReports(DateTime from, DateTime to)
    {
        _currentReport = new RepairReport { Title = "General report", From = from, To = to, Orders = Orders };
        return ReportService.BuildRevenueReport(Orders, from, to) + "\n"
            + ReportService.BuildPopularWorks(Orders) + "\n\n"
            + ReportService.BuildMechanicsLoad(Mechanics, Orders) + "\n"
            + ReportService.BuildPartsStock(Parts);
    }

    public List<RepairOrder> GetOrdersForMechanic(Mechanic m)
    {
        var result = new List<RepairOrder>();
        foreach (var id in m.AssignedOrderIds)
        {
            var o = Orders.FirstOrDefault(x => x.Id == id);
            if (o != null)
                result.Add(o);
        }
        return result;
    }

    public void NotifyAboutStatus(RepairOrder order, string type)
    {
        var phone = order.Customer?.Phone ?? "";
        var email = order.Customer?.Email ?? "";
        var text = $"Order {order.OrderNumber}: new status {order.Status}";
        if (type == "sms")
            SmsNotifier.SendSms(phone, text);
        else if (type == "email")
            EmailSender.Send(email, "Order status", text);
        else
        {
            SmsNotifier.SendSms(phone, text);
            EmailSender.Send(email, "Order status", text);
        }
        Notifications.Add($"{DateTime.Now:g}: {type} {text}");
    }

    private void Seed()
    {
        var c1 = AddCustomer("John Parker", "+1 555 100-20-30", "john@example.com", "12 Market Street");
        var c2 = AddCustomer("Anna Stone", "+1 555 555-44-33", "anna@example.com", "45 Lake Avenue");
        var car1 = AddCar(c1, "Toyota", "Camry", 2018, "JTNB11HK303000001", 87000, "ABC123");
        AddCar(c2, "Kia", "Rio", 2021, "Z94CB41ABMR000002", 43000, "MOR777");
        var m1 = AddMechanic("Sam Miller", "engine", 1200);
        AddMechanic("Owen Lane", "electrical", 1500);
        AddPart("Oil filter", "OF-100", 650, 12);
        AddPart("Brake pads", "BR-500", 3200, 5);
        var order = CreateOrder(c1, car1, "Knock on startup, diagnostics required", m1, "Diagnostics", "card");
        AddWorkToOrder(order, "Computer diagnostics", 1.5, 2500);
        SaveAll();
    }
}
