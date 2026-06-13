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

        var customer = new Customer { Name = "Jane Driver", Phone = "555-1000", Email = "jane@example.com", Address = "1 Main Street" };
        manager.AddCustomer(customer);

        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Ford", Model = "Focus", Year = 2019, Vin = "VIN123", Mileage = 42000, LicensePlate = "CAR123" };
        manager.AddCar(car);

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
        var customer = new Customer { Name = "Mark Owner", Phone = "555-2000", Email = "mark@example.com", Address = "2 Center Road" };
        manager.AddCustomer(customer);
        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Honda", Model = "Civic", Year = 2020, Vin = "VIN456", Mileage = 18000, LicensePlate = "CIV456" };
        manager.AddCar(car);
        var mechanic = new Mechanic { Name = "Alex Smith", Specialization = MechanicSpecialization.Engine, HourRate = 100 };
        manager.AddMechanic(mechanic);

        var order = manager.CreateOrder(customer, car, "Oil leak", mechanic, OrderStatus.Diagnostics, PaymentType.Cash);

        Assert.Equal(OrderStatus.Diagnostics, order.Status);
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal(car.Id, order.CarId);
        Assert.Contains(order.Id, mechanic.AssignedOrderIds);
        Assert.Contains(order.StatusHistory, x => x.Contains(OrderStatus.Diagnostics.ToString()));
    }

    [Fact]
    public void ChangeOrderStatusCalculatesCostAndSendsNotifications()
    {
        var manager = CreateManager();
        var customer = new Customer { Name = "Nina Client", Phone = "555-3000", Email = "nina@example.com", Address = "3 North Street" };
        manager.AddCustomer(customer);
        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Mazda", Model = "3", Year = 2017, Vin = "VIN789", Mileage = 99000, LicensePlate = "MAZ789" };
        manager.AddCar(car);
        var mechanic = new Mechanic { Name = "Chris Hall", Specialization = MechanicSpecialization.Electrical, HourRate = 200 };
        manager.AddMechanic(mechanic);
        var part = new Part { Name = "Sensor", Article = "SN-1", Price = 100, Stock = 4 };
        manager.AddPart(part);

        var order = manager.CreateOrder(customer, car, "Check warning light", mechanic, OrderStatus.Diagnostics, PaymentType.Card);

        manager.AddWorkToOrder(order, "Scanner diagnostics", 2, 300);
        Assert.True(manager.UsePartForOrder(order, part, 1));
        manager.ChangeOrderStatus(order, OrderStatus.Ready, "both");

        Assert.Equal(OrderStatus.Ready, order.Status);
        Assert.NotNull(order.CompletedAt);
        Assert.True(order.Cost > 0);
        Assert.Contains(manager.Notifier.Log, x => x.Contains("Ready"));
        Assert.Equal(3, part.Stock);
    }

    [Fact]
    public void UsePartReturnsFalseWhenStockIsTooLow()
    {
        var manager = CreateManager();
        var customer = new Customer { Name = "Low Stock", Phone = "555-4000", Email = "low@example.com", Address = "4 South Street" };
        manager.AddCustomer(customer);
        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Nissan", Model = "Leaf", Year = 2022, Vin = "VIN000", Mileage = 12000, LicensePlate = "EV100" };
        manager.AddCar(car);

        var order = manager.CreateOrder(customer, car, "Noise", null, OrderStatus.New, PaymentType.Cash);
        var part = new Part { Name = "Battery clamp", Article = "BC-1", Price = 25, Stock = 1 };
        manager.AddPart(part);

        var used = manager.UsePartForOrder(order, part, 2);

        Assert.False(used);
        Assert.Equal(1, part.Stock);
        Assert.Empty(order.UsedPartIds);
    }

    [Fact]
    public void DeleteCarRemovesCustomerLinkAndRelatedOrders()
    {
        var manager = CreateManager();
        var customer = new Customer { Name = "Delete Car", Phone = "555-4100", Email = "delete@example.com", Address = "41 Garage Road" };
        manager.AddCustomer(customer);
        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Volkswagen", Model = "Golf", Year = 2015, Vin = "VINDEL", Mileage = 71000, LicensePlate = "DEL123" };
        manager.AddCar(car);

        var order = manager.CreateOrder(customer, car, "Remove with car", null, OrderStatus.New, PaymentType.Cash);

        manager.DeleteCar(car);

        Assert.DoesNotContain(manager.Cars, x => x.Id == car.Id);
        Assert.DoesNotContain(customer.Cars, x => x.Id == car.Id);
        Assert.DoesNotContain(manager.Orders, x => x.Id == order.Id);
    }

    [Fact]
    public void ReportsContainRevenueWorkloadAndStockSections()
    {
        var manager = CreateManager();
        var customer = new Customer { Name = "Report Client", Phone = "555-5000", Email = "report@example.com", Address = "5 West Street" };
        manager.AddCustomer(customer);
        var car = new Car { Owner = customer, CustomerId = customer.Id, Make = "Subaru", Model = "Outback", Year = 2016, Vin = "VIN111", Mileage = 110000, LicensePlate = "SUB111" };
        manager.AddCar(car);
        var mechanic = new Mechanic { Name = "Dana Ray", Specialization = MechanicSpecialization.General, HourRate = 150 };
        manager.AddMechanic(mechanic);
        var part = new Part { Name = "Gasket", Article = "GS-1", Price = 15, Stock = 2 };
        manager.AddPart(part);
        var order = manager.CreateOrder(customer, car, "Service", mechanic, OrderStatus.Ready, PaymentType.Cash);
        manager.AddWorkToOrder(order, "Inspection", 1, 80);

        var report = manager.ReportService.BuildReports(manager.Orders, manager.Mechanics, manager.Parts, new DateRange(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)));

        Assert.Contains("Revenue for period", report);
        Assert.Contains("Popular services", report);
        Assert.Contains("Mechanic workload", report);
        Assert.Contains("Parts stock", report);
    }

    [Fact]
    public void JsonStoreSavesAndLoadsPlainEntities()
    {
        var folder = MakeTempFolder();
        var store = new JsonFileStore<Customer>();
        store.SetTestFolder(folder);

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
        var manager = new AutoServiceManager();

        manager.CustomerStore.SetTestFolder(folder);
        manager.CarStore.SetTestFolder(folder);
        manager.OrderStore.SetTestFolder(folder);
        manager.PartStore.SetTestFolder(folder);
        manager.MechanicStore.SetTestFolder(folder);

        return manager;
    }

    private static string MakeTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "AutoServiceAppTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}