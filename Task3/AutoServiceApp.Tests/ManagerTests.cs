using AutoServiceApp.Models;
using AutoServiceApp.Services;
using AutoServiceApp.Storage;

namespace AutoServiceApp.Tests;

public class ManagerTests
{
    [Fact]
    public void AddCustomerAndCarLinksOwnerAndPersistsIds()
    {
        var manager = CreateManager();

        var customer = manager.AddCustomer("Jane Driver", "555-1000", "jane@example.com", "1 Main Street");
        var car = manager.AddCar(customer, "Ford", "Focus", 2019, "VIN123", 42000, "CAR123");

        Assert.Single(manager.Customers);
        Assert.Single(manager.Cars);
        Assert.Equal(customer.Id, car.CustomerId);
        Assert.Contains(manager.Customers[0].Cars, x => x.Id == car.Id);
        Assert.Same(customer, car.Owner);
    }

    [Fact]
    public void CreateOrderStoresHistoryAndMechanicAssignment()
    {
        var manager = CreateManager();
        var customer = manager.AddCustomer("Mark Owner", "555-2000", "mark@example.com", "2 Center Road");
        var car = manager.AddCar(customer, "Honda", "Civic", 2020, "VIN456", 18000, "CIV456");
        var mechanic = manager.AddMechanic("Alex Smith", "engine", 100);

        var order = manager.CreateOrder(customer, car, "Oil leak", mechanic, "Diagnostics", "cash");

        Assert.Equal("Diagnostics", order.Status);
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal(car.Id, order.CarId);
        Assert.Contains(order.Id, mechanic.AssignedOrderIds);
        Assert.Contains(order.StatusHistory, x => x.Contains("order created with status Diagnostics"));
    }

    [Fact]
    public void ChangeOrderStatusCalculatesCostAndSendsNotifications()
    {
        var manager = CreateManager();
        var customer = manager.AddCustomer("Nina Client", "555-3000", "nina@example.com", "3 North Street");
        var car = manager.AddCar(customer, "Mazda", "3", 2017, "VIN789", 99000, "MAZ789");
        var mechanic = manager.AddMechanic("Chris Hall", "electrical", 200);
        var part = manager.AddPart("Sensor", "SN-1", 100, 4);
        var order = manager.CreateOrder(customer, car, "Check warning light", mechanic, "Diagnostics", "card");

        manager.AddWorkToOrder(order, "Scanner diagnostics", 2, 300);
        Assert.True(manager.UsePartForOrder(order, part, 1));
        manager.ChangeOrderStatus(order, "Ready", "both");

        Assert.Equal("Ready", order.Status);
        Assert.NotNull(order.CompletedAt);
        Assert.True(order.Cost > 0);
        Assert.Contains(manager.SmsNotifier.SentMessages, x => x.Contains("new status Ready"));
        Assert.Contains(manager.EmailSender.Log, x => x.Contains("Order status"));
        Assert.Equal(3, part.Stock);
    }

    [Fact]
    public void UsePartReturnsFalseWhenStockIsTooLow()
    {
        var manager = CreateManager();
        var customer = manager.AddCustomer("Low Stock", "555-4000", "low@example.com", "4 South Street");
        var car = manager.AddCar(customer, "Nissan", "Leaf", 2022, "VIN000", 12000, "EV100");
        var order = manager.CreateOrder(customer, car, "Noise", null, "New", "cash");
        var part = manager.AddPart("Battery clamp", "BC-1", 25, 1);

        var used = manager.UsePartForOrder(order, part, 2);

        Assert.False(used);
        Assert.Equal(1, part.Stock);
        Assert.Empty(order.UsedPartIds);
    }

    [Fact]
    public void DeleteCarRemovesCustomerLinkAndRelatedOrders()
    {
        var manager = CreateManager();
        var customer = manager.AddCustomer("Delete Car", "555-4100", "delete@example.com", "41 Garage Road");
        var car = manager.AddCar(customer, "Volkswagen", "Golf", 2015, "VINDEL", 71000, "DEL123");
        var order = manager.CreateOrder(customer, car, "Remove with car", null, "New", "cash");

        manager.DeleteCar(car);

        Assert.DoesNotContain(manager.Cars, x => x.Id == car.Id);
        Assert.DoesNotContain(customer.Cars, x => x.Id == car.Id);
        Assert.DoesNotContain(manager.Orders, x => x.Id == order.Id);
    }

    [Fact]
    public void ReportsContainRevenueWorkloadAndStockSections()
    {
        var manager = CreateManager();
        var customer = manager.AddCustomer("Report Client", "555-5000", "report@example.com", "5 West Street");
        var car = manager.AddCar(customer, "Subaru", "Outback", 2016, "VIN111", 110000, "SUB111");
        var mechanic = manager.AddMechanic("Dana Ray", "transmission", 150);
        manager.AddPart("Gasket", "GS-1", 15, 2);
        var order = manager.CreateOrder(customer, car, "Service", mechanic, "Ready", "cash");
        manager.AddWorkToOrder(order, "Inspection", 1, 80);

        var report = manager.BuildReports(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

        Assert.Contains("Revenue for period", report);
        Assert.Contains("Popular services", report);
        Assert.Contains("Mechanic workload", report);
        Assert.Contains("Parts stock", report);
    }

    [Fact]
    public void JsonStoreSavesAndLoadsPlainEntities()
    {
        var folder = MakeTempFolder();
        var store = new JsonFileStore<Customer> { Folder = folder };
        var customers = new List<Customer>
        {
            new() { Name = "Saved Customer", Phone = "555", Email = "saved@example.com", Address = "Saved address" }
        };

        store.Save("customers.json", customers);
        var loaded = store.Load("customers.json");

        Assert.Single(loaded);
        Assert.Equal("Saved Customer", loaded[0].Name);
    }

    private static AutoServiceManager CreateManager()
    {
        var folder = MakeTempFolder();
        return new AutoServiceManager
        {
            CustomerStore = new JsonFileStore<Customer> { Folder = folder },
            CarStore = new JsonFileStore<Car> { Folder = folder },
            OrderStore = new JsonFileStore<RepairOrder> { Folder = folder },
            PartStore = new JsonFileStore<Part> { Folder = folder },
            MechanicStore = new JsonFileStore<Mechanic> { Folder = folder }
        };
    }

    private static string MakeTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "AutoServiceAppTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
