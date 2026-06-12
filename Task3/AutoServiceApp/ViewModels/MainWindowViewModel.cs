using AutoServiceApp.Services;

namespace AutoServiceApp.ViewModels;

public class MainWindowViewModel
{
    public AutoServiceManager Manager { get; set; } = new();
    public string Title { get; set; } = "Auto Service";
}
