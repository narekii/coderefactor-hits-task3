using System.Text;
using AutoServiceApp.Helpers;
using AutoServiceApp.Models;
using AutoServiceApp.Storage;

namespace AutoServiceApp.Services;

public class AutoServiceManager
{
    public List<Customer> Customers { get; private set; } = new();
    public List<Car> Cars { get; private set; } = new();
    public List<RepairOrder> Orders { get; private set; } = new();
    public List<Part> Parts { get; private set; } = new();
    public List<Mechanic> Mechanics { get; private set; } = new();
    public List<string> Notifications { get; private set; } = new();
    public JsonFileStore<Customer> CustomerStore { get; set; } = new();
    public JsonFileStore<Car> CarStore { get; set; } = new();
    public JsonFileStore<RepairOrder> OrderStore { get; set; } = new();
    public JsonFileStore<Part> PartStore { get; set; } = new();
    public JsonFileStore<Mechanic> MechanicStore { get; set; } = new();
    public ReportService ReportService { get; set; } = new();
    public OrderStatusHelper StatusHelper { get; set; } = new();
    public PricingService Pricing { get; set; } = new();
    public NotificationService Notifier { get; set; } = new();

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

    public void AddCustomer(Customer customer)
    {
        Customers.Add(customer);
        SaveAll();
    }

    public void UpdateCustomer(Customer customer)
    {
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

    public void AddCar(Car car)
    {
        Cars.Add(car);
        car.Owner?.Cars.Add(car);
        SaveAll();
    }

    public void UpdateCar(Car car)
    {
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

    public void AddMechanic(Mechanic mechanic)
    {
        Mechanics.Add(mechanic);
        SaveAll();
    }

    public void UpdateMechanic(Mechanic mechanic)
    {
        SaveAll();
    }

    public void DeleteMechanic(Mechanic mechanic)
    {
        Mechanics.Remove(mechanic);
        foreach (var order in Orders.Where(o => o.AssignedMechanicId == mechanic.Id))
        {
            order.AssignedMechanicId = "";
            order.AssignedMechanic = null;
        }
        SaveAll();
    }

    public void AddPart(Part part)
    {
        Parts.Add(part);
        SaveAll();
    }

    public void UpdatePart(Part part)
    {
        SaveAll();
    }

    public void DeletePart(Part p)
    {
        Parts.Remove(p);
        SaveAll();
    }

    public RepairOrder CreateOrder(Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, PaymentType paymentMethod)
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

    public void UpdateOrder(RepairOrder order, Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, decimal cost, PaymentType paymentMethod)
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

    public void ChangeOrderStatus(RepairOrder order, OrderStatus newStatus, string notificationType)
    {
        order.Status = newStatus;
        order.StatusHistory.Add($"{DateTime.Now:g}: status changed to {newStatus}");
        if (newStatus == OrderStatus.Ready)
            order.CompletedAt = DateTime.Now;

        if (newStatus == OrderStatus.Ready)
            order.Cost = Pricing.CalculateOrderCost(order, true, order.PaymentMethod);

        if (order.AssignedMechanic != null && !order.AssignedMechanic.AssignedOrderIds.Contains(order.Id))
            order.AssignedMechanic.AssignedOrderIds.Add(order.Id);

        NotifyAboutStatus(order, notificationType);
        SaveAll();
    }

    public void AddWorkToOrder(RepairOrder order, string name, double hours, decimal cost)
    {
        var work = new RepairWork { Name = name, Hours = hours, Cost = cost };
        order.Works.Add(work);
        order.Cost = Pricing.CalculateOrderCost(order, false, order.PaymentMethod);
        SaveAll();
    }

    public bool UsePartForOrder(RepairOrder order, Part part, int qty)
    {
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
        Notifier.Notify(type, phone, email, "Order status", text);
        Notifications.Add($"{DateTime.Now:g}: {type} {text}");
    }

    private void Seed()
    {
        var c1 = new Customer { Name = "John Parker", Phone = "+1 555 100-20-30", Email = "john@example.com", Address = "12 Market Street" };
        var c2 = new Customer { Name = "Anna Stone", Phone = "+1 555 555-44-33", Email = "anna@example.com", Address = "45 Lake Avenue" };
        AddCustomer(c1);
        AddCustomer(c2);
        var car1 = new Car { Owner = c1, CustomerId = c1.Id, Make = "Toyota", Model = "Camry", Year = 2018, Vin = "JTNB11HK303000001", Mileage = 87000, LicensePlate = "ABC123" };
        var car2 = new Car { Owner = c2, CustomerId = c2.Id, Make = "Kia", Model = "Rio", Year = 2021, Vin = "Z94CB41ABMR000002", Mileage = 43000, LicensePlate = "MOR777" };
        AddCar(car1);
        AddCar(car2);
        var m1 = new Mechanic { Name = "Sam Miller", Specialization = "engine", HourRate = 1200 };
        var m2 = new Mechanic { Name = "Owen Lane", Specialization = "electrical", HourRate = 1500 };
        AddMechanic(m1);
        AddMechanic(m2);
        var p1 = new Part { Name = "Oil filter", Article = "OF-100", Price = 650, Stock = 12 };
        var p2 = new Part { Name = "Brake pads", Article = "BR-500", Price = 3200, Stock = 5 };
        AddPart(p1);
        AddPart(p2);
        var order = CreateOrder(c1, car1, "Knock on startup, diagnostics required", m1, OrderStatus.Diagnostics, PaymentType.Card);
        AddWorkToOrder(order, "Computer diagnostics", 1.5, 2500);
        SaveAll();
    }
}
