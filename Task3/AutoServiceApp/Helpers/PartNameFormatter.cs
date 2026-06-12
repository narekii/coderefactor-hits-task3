using AutoServiceApp.Models;

namespace AutoServiceApp.Helpers;

public class PartNameFormatter
{
    public string Pretty(Part part) => $"{part.Name} ({part.Article})";
}
