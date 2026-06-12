using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using AutoServiceApp.Helpers;
using AutoServiceApp.Models;
using AutoServiceApp.Services;

namespace AutoServiceApp;

public partial class MainWindow : Window
{
    public AutoServiceManager Manager { get; set; } = new();

    private ListBox _customerList = new();
    private ListBox _carList = new();
    private ListBox _orderList = new();
    private ListBox _partList = new();
    private ListBox _mechanicList = new();
    private ListBox _orderWorkList = new();
    private TextBox _notificationLog = new();
    private TextBox _reportText = new();
    private TextBox _orderDetailsText = new();

    private TextBox _customerName = new();
    private TextBox _customerPhone = new();
    private TextBox _customerEmail = new();
    private TextBox _customerAddress = new();

    private ComboBox _carCustomer = new();
    private TextBox _carMake = new();
    private TextBox _carModel = new();
    private TextBox _carYear = new();
    private TextBox _carVin = new();
    private TextBox _carLicense = new();
    private TextBox _carMileage = new();

    private ComboBox _orderCustomer = new();
    private ComboBox _orderCar = new();
    private ComboBox _orderMechanic = new();
    private ComboBox _orderStatus = new();
    private ComboBox _orderPayment = new();
    private TextBox _orderProblem = new();
    private TextBox _orderCost = new();
    private TextBox _workName = new();
    private TextBox _workHours = new();
    private TextBox _workCost = new();
    private ComboBox _usePartCombo = new();
    private TextBox _usePartQty = new();

    private TextBox _partName = new();
    private TextBox _partArticle = new();
    private TextBox _partPrice = new();
    private TextBox _partStock = new();

    private TextBox _mechanicName = new();
    private TextBox _mechanicSpec = new();
    private TextBox _mechanicRate = new();
    private TextBox _mechanicOrders = new();

    public MainWindow()
    {
        InitializeComponent();
        Manager.Load();
        BuildUi();
        RefreshAll();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BuildUi()
    {
        var root = this.FindControl<Grid>("RootPanel");
        if (root == null)
            return;

        var tabs = new TabControl
        {
            Items =
            {
                new TabItem { Header = "Home", Content = BuildHomeTab() },
                new TabItem { Header = "Customers", Content = BuildCustomersTab() },
                new TabItem { Header = "Cars", Content = BuildCarsTab() },
                new TabItem { Header = "Orders", Content = BuildOrdersTab() },
                new TabItem { Header = "Stock", Content = BuildPartsTab() },
                new TabItem { Header = "Mechanics", Content = BuildMechanicsTab() },
                new TabItem { Header = "Reports", Content = BuildReportsTab() }
            }
        };
        root.Children.Add(tabs);
    }

    private Control BuildHomeTab()
    {
        var panel = new StackPanel { Spacing = 12, Margin = new Avalonia.Thickness(8) };
        panel.Children.Add(new TextBlock { Text = "Auto service: training management system", FontSize = 24 });
        panel.Children.Add(new TextBlock { Text = "Data is automatically loaded from and saved to JSON files in the user profile.", TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = "Notification log" });
        _notificationLog = new TextBox { AcceptsReturn = true, IsReadOnly = true, MinHeight = 420 };
        panel.Children.Add(_notificationLog);
        return new ScrollViewer { Content = panel };
    }

    private Control BuildCustomersTab()
    {
        var grid = TwoColumnGrid();
        var form = FormPanel();
        _customerName = Box("Name");
        _customerPhone = Box("Phone");
        _customerEmail = Box("Email");
        _customerAddress = Box("Address");
        AddLabeled(form, "Name", _customerName);
        AddLabeled(form, "Phone", _customerPhone);
        AddLabeled(form, "Email", _customerEmail);
        AddLabeled(form, "Address", _customerAddress);
        form.Children.Add(RowButtons(
            ("Create", (_, _) => { Manager.AddCustomer(_customerName.Text ?? "", _customerPhone.Text ?? "", _customerEmail.Text ?? "", _customerAddress.Text ?? ""); ClearCustomerForm(); RefreshAll(); }),
            ("Save", (_, _) => { if (_customerList.SelectedItem is Customer c) { Manager.UpdateCustomer(c, _customerName.Text ?? "", _customerPhone.Text ?? "", _customerEmail.Text ?? "", _customerAddress.Text ?? ""); RefreshAll(); } }),
            ("Delete", (_, _) => { if (_customerList.SelectedItem is Customer c) { _customerList.ItemsSource = null; Manager.DeleteCustomer(c); ClearCustomerForm(); RefreshAll(); } })));
        Grid.SetColumn(form, 0);
        grid.Children.Add(form);

        _customerList = new ListBox();
        _customerList.SelectionChanged += (_, _) =>
        {
            if (_customerList.SelectedItem is Customer c)
            {
                _customerName.Text = c.Name;
                _customerPhone.Text = c.Phone;
                _customerEmail.Text = c.Email;
                _customerAddress.Text = c.Address;
            }
        };
        Grid.SetColumn(_customerList, 1);
        grid.Children.Add(_customerList);
        return grid;
    }

    private Control BuildCarsTab()
    {
        var grid = TwoColumnGrid();
        var form = FormPanel();
        _carCustomer = new ComboBox { PlaceholderText = "Customer" };
        _carMake = Box("Make");
        _carModel = Box("Model");
        _carYear = Box("Year");
        _carVin = Box("VIN");
        _carLicense = Box("License plate");
        _carMileage = Box("Mileage");
        AddLabeled(form, "Customer", _carCustomer);
        AddLabeled(form, "Make", _carMake);
        AddLabeled(form, "Model", _carModel);
        AddLabeled(form, "Year", _carYear);
        AddLabeled(form, "VIN", _carVin);
        AddLabeled(form, "License plate", _carLicense);
        AddLabeled(form, "Mileage", _carMileage);
        form.Children.Add(RowButtons(
            ("Create", (_, _) => { Manager.AddCar(_carCustomer.SelectedItem as Customer, _carMake.Text ?? "", _carModel.Text ?? "", Int(_carYear.Text), _carVin.Text ?? "", Int(_carMileage.Text), _carLicense.Text ?? ""); ClearCarForm(); RefreshAll(); }),
            ("Save", (_, _) => { if (_carList.SelectedItem is Car car) { Manager.UpdateCar(car, _carCustomer.SelectedItem as Customer, _carMake.Text ?? "", _carModel.Text ?? "", Int(_carYear.Text), _carVin.Text ?? "", Int(_carMileage.Text), _carLicense.Text ?? ""); RefreshAll(); } }),
            ("Delete", (_, _) => { if (_carList.SelectedItem is Car car) { _carList.ItemsSource = null; Manager.DeleteCar(car); ClearCarForm(); RefreshAll(); } })));
        Grid.SetColumn(form, 0);
        grid.Children.Add(form);
        _carList = new ListBox();
        _carList.SelectionChanged += (_, _) =>
        {
            if (_carList.SelectedItem is Car car)
            {
                _carCustomer.SelectedItem = Manager.Customers.FirstOrDefault(x => x.Id == car.CustomerId);
                _carMake.Text = car.Make;
                _carModel.Text = car.Model;
                _carYear.Text = car.Year.ToString();
                _carVin.Text = car.Vin;
                _carLicense.Text = car.LicensePlate;
                _carMileage.Text = car.Mileage.ToString();
            }
        };
        Grid.SetColumn(_carList, 1);
        grid.Children.Add(_carList);
        return grid;
    }

    private Control BuildOrdersTab()
    {
        var grid = TwoColumnGrid(420);
        var form = FormPanel();
        _orderCustomer = new ComboBox { PlaceholderText = "Customer" };
        _orderCar = new ComboBox { PlaceholderText = "Car" };
        _orderMechanic = new ComboBox { PlaceholderText = "Mechanic" };
        _orderStatus = new ComboBox { ItemsSource = new[] { "New", "Diagnostics", "In Progress", "Waiting for Parts", "Ready", "Released" }, SelectedIndex = 0 };
        _orderPayment = new ComboBox { ItemsSource = new[] { "cash", "card", "transfer" }, SelectedIndex = 0 };
        _orderProblem = Box("Problem description");
        _orderCost = Box("Cost");
        AddLabeled(form, "Customer", _orderCustomer);
        AddLabeled(form, "Car", _orderCar);
        AddLabeled(form, "Mechanic", _orderMechanic);
        AddLabeled(form, "Status", _orderStatus);
        AddLabeled(form, "Payment", _orderPayment);
        AddLabeled(form, "Description", _orderProblem);
        AddLabeled(form, "Cost", _orderCost);
        form.Children.Add(RowButtons(
            ("Create", (_, _) => { Manager.CreateOrder(_orderCustomer.SelectedItem as Customer, _orderCar.SelectedItem as Car, _orderProblem.Text ?? "", _orderMechanic.SelectedItem as Mechanic, _orderStatus.SelectedItem?.ToString() ?? "New", _orderPayment.SelectedItem?.ToString() ?? "cash"); ClearOrderForm(); RefreshAll(); }),
            ("Save", (_, _) => { if (_orderList.SelectedItem is RepairOrder o) { Manager.UpdateOrder(o, _orderCustomer.SelectedItem as Customer, _orderCar.SelectedItem as Car, _orderProblem.Text ?? "", _orderMechanic.SelectedItem as Mechanic, _orderStatus.SelectedItem?.ToString() ?? "New", Decimal(_orderCost.Text), _orderPayment.SelectedItem?.ToString() ?? "cash"); RefreshAll(); } }),
            ("Delete", (_, _) => { if (_orderList.SelectedItem is RepairOrder o) { _orderList.ItemsSource = null; Manager.Orders.Remove(o); Manager.SaveAll(); ClearOrderForm(); RefreshAll(); } })));

        form.Children.Add(new TextBlock { Text = "Add work", Margin = new Avalonia.Thickness(0, 12, 0, 0) });
        _workName = Box("Work name");
        _workHours = Box("Hours");
        _workCost = Box("Cost");
        AddLabeled(form, "Work", _workName);
        AddLabeled(form, "Hours", _workHours);
        AddLabeled(form, "Price", _workCost);
        form.Children.Add(Button("Add work", (_, _) => { if (_orderList.SelectedItem is RepairOrder o) { Manager.AddWorkToOrder(o, _workName.Text ?? "", Double(_workHours.Text), Decimal(_workCost.Text)); RefreshAll(); SelectOrder(o); } }));

        form.Children.Add(new TextBlock { Text = "Use part", Margin = new Avalonia.Thickness(0, 12, 0, 0) });
        _usePartCombo = new ComboBox { PlaceholderText = "Part" };
        _usePartQty = Box("Quantity");
        AddLabeled(form, "Part", _usePartCombo);
        AddLabeled(form, "Qty", _usePartQty);
        form.Children.Add(Button("Use", (_, _) => { if (_orderList.SelectedItem is RepairOrder o && _usePartCombo.SelectedItem is Part p) { Manager.UsePartForOrder(o, p, Math.Max(1, Int(_usePartQty.Text))); RefreshAll(); SelectOrder(o); } }));
        Grid.SetColumn(form, 0);
        grid.Children.Add(new ScrollViewer { Content = form });

        var right = new Grid();
        right.RowDefinitions.Add(new RowDefinition(new GridLength(260)));
        right.RowDefinitions.Add(new RowDefinition(new GridLength(120)));
        right.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _orderList = new ListBox();
        _orderList.SelectionChanged += (_, _) =>
        {
            if (_orderList.SelectedItem is RepairOrder o)
                FillOrder(o);
        };
        right.Children.Add(_orderList);
        _orderWorkList = new ListBox();
        Grid.SetRow(_orderWorkList, 1);
        right.Children.Add(_orderWorkList);
        _orderDetailsText = new TextBox { AcceptsReturn = true, IsReadOnly = true };
        Grid.SetRow(_orderDetailsText, 2);
        right.Children.Add(_orderDetailsText);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    private Control BuildPartsTab()
    {
        var grid = TwoColumnGrid();
        var form = FormPanel();
        _partName = Box("Name");
        _partArticle = Box("Article");
        _partPrice = Box("Price");
        _partStock = Box("Stock");
        AddLabeled(form, "Name", _partName);
        AddLabeled(form, "Article", _partArticle);
        AddLabeled(form, "Price", _partPrice);
        AddLabeled(form, "Stock", _partStock);
        form.Children.Add(RowButtons(
            ("Add", (_, _) => { Manager.AddPart(_partName.Text ?? "", _partArticle.Text ?? "", Decimal(_partPrice.Text), Int(_partStock.Text)); ClearPartForm(); RefreshAll(); }),
            ("Save", (_, _) => { if (_partList.SelectedItem is Part p) { Manager.UpdatePart(p, _partName.Text ?? "", _partArticle.Text ?? "", Decimal(_partPrice.Text), Int(_partStock.Text)); RefreshAll(); } }),
            ("Delete", (_, _) => { if (_partList.SelectedItem is Part p) { _partList.ItemsSource = null; Manager.DeletePart(p); ClearPartForm(); RefreshAll(); } })));
        Grid.SetColumn(form, 0);
        grid.Children.Add(form);
        _partList = new ListBox();
        _partList.SelectionChanged += (_, _) =>
        {
            if (_partList.SelectedItem is Part p)
            {
                _partName.Text = p.Name;
                _partArticle.Text = p.Article;
                _partPrice.Text = p.Price.ToString();
                _partStock.Text = p.Stock.ToString();
            }
        };
        Grid.SetColumn(_partList, 1);
        grid.Children.Add(_partList);
        return grid;
    }

    private Control BuildMechanicsTab()
    {
        var grid = TwoColumnGrid();
        var form = FormPanel();
        _mechanicName = Box("Name");
        _mechanicSpec = Box("Specialization");
        _mechanicRate = Box("Rate");
        _mechanicOrders = new TextBox { AcceptsReturn = true, IsReadOnly = true, MinHeight = 200 };
        AddLabeled(form, "Name", _mechanicName);
        AddLabeled(form, "Specialization", _mechanicSpec);
        AddLabeled(form, "Rate", _mechanicRate);
        form.Children.Add(RowButtons(
            ("Create", (_, _) => { Manager.AddMechanic(_mechanicName.Text ?? "", _mechanicSpec.Text ?? "", Decimal(_mechanicRate.Text)); ClearMechanicForm(); RefreshAll(); }),
            ("Save", (_, _) => { if (_mechanicList.SelectedItem is Mechanic m) { Manager.UpdateMechanic(m, _mechanicName.Text ?? "", _mechanicSpec.Text ?? "", Decimal(_mechanicRate.Text)); RefreshAll(); } }),
            ("Delete", (_, _) => { if (_mechanicList.SelectedItem is Mechanic m) { _mechanicList.ItemsSource = null; Manager.DeleteMechanic(m); ClearMechanicForm(); RefreshAll(); } })));
        form.Children.Add(new TextBlock { Text = "Assigned orders" });
        form.Children.Add(_mechanicOrders);
        Grid.SetColumn(form, 0);
        grid.Children.Add(form);
        _mechanicList = new ListBox();
        _mechanicList.SelectionChanged += (_, _) =>
        {
            if (_mechanicList.SelectedItem is Mechanic m)
            {
                _mechanicName.Text = m.Name;
                _mechanicSpec.Text = m.Specialization;
                _mechanicRate.Text = m.HourRate.ToString();
                _mechanicOrders.Text = string.Join("\n", Manager.GetOrdersForMechanic(m).Select(x => x.ToString()));
            }
        };
        Grid.SetColumn(_mechanicList, 1);
        grid.Children.Add(_mechanicList);
        return grid;
    }

    private Control BuildReportsTab()
    {
        var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(8) };
        var from = Box(DateTime.Today.AddMonths(-1).ToShortDateString());
        var to = Box(DateTime.Today.ToShortDateString());
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(new TextBlock { Text = "From", VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(from);
        row.Children.Add(new TextBlock { Text = "To", VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(to);
        row.Children.Add(Button("Build", (_, _) =>
        {
            var f = DateTime.TryParse(from.Text, out var fd) ? fd : DateTime.Today.AddMonths(-1);
            var t = DateTime.TryParse(to.Text, out var td) ? td : DateTime.Today;
            _reportText.Text = Manager.BuildReports(f, t);
        }));
        panel.Children.Add(row);
        _reportText = new TextBox { AcceptsReturn = true, IsReadOnly = true, MinHeight = 600 };
        panel.Children.Add(_reportText);
        return panel;
    }

    private Grid TwoColumnGrid(double left = 360)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(left)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        return grid;
    }

    private StackPanel FormPanel() => new() { Spacing = 7, Margin = new Avalonia.Thickness(8) };

    private TextBox Box(string watermark) => new() { Watermark = watermark };

    private Button Button(string text, EventHandler<RoutedEventArgs> handler)
    {
        var b = new Button { Content = text, MinWidth = 90 };
        b.Click += handler;
        return b;
    }

    private StackPanel RowButtons(params (string Text, EventHandler<RoutedEventArgs> Handler)[] buttons)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 8, 0, 0) };
        foreach (var b in buttons)
            row.Children.Add(Button(b.Text, b.Handler));
        return row;
    }

    private void AddLabeled(StackPanel panel, string label, Control control)
    {
        panel.Children.Add(new TextBlock { Text = label });
        panel.Children.Add(control);
    }

    private void RefreshAll()
    {
        Manager.RelinkEverything();
        _customerList.ItemsSource = null;
        _customerList.ItemsSource = Manager.Customers.ToList();
        _carList.ItemsSource = null;
        _carList.ItemsSource = Manager.Cars.ToList();
        _orderList.ItemsSource = null;
        _orderList.ItemsSource = Manager.Orders.ToList();
        _partList.ItemsSource = null;
        _partList.ItemsSource = Manager.Parts.ToList();
        _mechanicList.ItemsSource = null;
        _mechanicList.ItemsSource = Manager.Mechanics.ToList();
        _carCustomer.ItemsSource = Manager.Customers.ToList();
        _orderCustomer.ItemsSource = Manager.Customers.ToList();
        _orderCar.ItemsSource = Manager.Cars.ToList();
        _orderMechanic.ItemsSource = Manager.Mechanics.ToList();
        _usePartCombo.ItemsSource = Manager.Parts.ToList();
        _notificationLog.Text = string.Join(Environment.NewLine, Manager.Notifications.Concat(Manager.SmsNotifier.SentMessages).Concat(Manager.EmailSender.Log));
        _reportText.Text = Manager.BuildReports(DateTime.Today.AddMonths(-1), DateTime.Today);
    }

    private void FillOrder(RepairOrder o)
    {
        _orderCustomer.SelectedItem = Manager.Customers.FirstOrDefault(x => x.Id == o.CustomerId);
        _orderCar.SelectedItem = Manager.Cars.FirstOrDefault(x => x.Id == o.CarId);
        _orderMechanic.SelectedItem = Manager.Mechanics.FirstOrDefault(x => x.Id == o.AssignedMechanicId);
        _orderStatus.SelectedItem = o.Status;
        _orderPayment.SelectedItem = o.PaymentMethod;
        _orderProblem.Text = o.ProblemDescription;
        _orderCost.Text = o.Cost.ToString();
        _orderWorkList.ItemsSource = null;
        _orderWorkList.ItemsSource = o.Works;
        _orderDetailsText.Text = Manager.BuildOrderDetails(o);
    }

    private void SelectOrder(RepairOrder o)
    {
        _orderList.SelectedItem = o;
        FillOrder(o);
    }

    private void ClearCustomerForm() => (_customerName.Text, _customerPhone.Text, _customerEmail.Text, _customerAddress.Text) = ("", "", "", "");
    private void ClearCarForm() => (_carMake.Text, _carModel.Text, _carYear.Text, _carVin.Text, _carLicense.Text, _carMileage.Text) = ("", "", "", "", "", "");
    private void ClearOrderForm() => (_orderProblem.Text, _orderCost.Text, _workName.Text, _workHours.Text, _workCost.Text, _usePartQty.Text) = ("", "", "", "", "", "");
    private void ClearPartForm() => (_partName.Text, _partArticle.Text, _partPrice.Text, _partStock.Text) = ("", "", "", "");
    private void ClearMechanicForm() => (_mechanicName.Text, _mechanicSpec.Text, _mechanicRate.Text, _mechanicOrders.Text) = ("", "", "", "");

    private int Int(string? text) => int.TryParse(text, out var x) ? x : 0;
    private double Double(string? text) => double.TryParse(text, out var x) ? x : 0;
    private decimal Decimal(string? text) => decimal.TryParse(text, out var x) ? x : 0;
}
