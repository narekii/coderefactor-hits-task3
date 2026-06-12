using AutoServiceApp.Models;

namespace AutoServiceApp.Helpers;

public class MechanicDisplayHelper
{
    public string GetText(Mechanic mechanic) => mechanic.Name + " - " + mechanic.Specialization;
}
