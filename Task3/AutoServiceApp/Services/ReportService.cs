using System.Text;
using AutoServiceApp.Models;

namespace AutoServiceApp.Services;

public class ReportService
{
    public string BuildRevenueReport(List<RepairOrder> orders, DateTime from, DateTime to)
    {
        var result = new StringBuilder();
        var selected = orders.Where(o => o.AcceptedAt.Date >= from.Date && o.AcceptedAt.Date <= to.Date).ToList();
        result.AppendLine($"Revenue for period {from:d} - {to:d}: {selected.Sum(x => x.Cost):C}");
        result.AppendLine($"Orders: {selected.Count}");
        result.AppendLine($"With service multiplier: {(selected.Sum(x => x.Cost) * 1.20m):C}");
        return result.ToString();
    }

    public string BuildPopularWorks(List<RepairOrder> orders)
    {
        var lines = orders.SelectMany(x => x.Works)
            .GroupBy(x => x.Name)
            .OrderByDescending(x => x.Count())
            .Select(x => $"{x.Key}: {x.Count()} times, {x.Sum(y => y.Cost):C}");
        return "Popular services\n" + string.Join("\n", lines);
    }

    public string BuildMechanicsLoad(List<Mechanic> mechanics, List<RepairOrder> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Mechanic workload");
        foreach (var m in mechanics)
        {
            var count = orders.Count(o => o.AssignedMechanicId == m.Id && o.Status != "Released");
            var bonus = count > 5 ? 1000 : 0;
            sb.AppendLine($"{m.Name}: active orders {count}, estimated bonus {bonus}");
        }
        return sb.ToString();
    }

    public string BuildPartsStock(List<Part> parts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Parts stock");
        foreach (var p in parts.OrderBy(x => x.Stock))
        {
            var line = p.Stock < 3 ? $"{p.Name} [{p.Article}] stock {p.Stock}, reorder at least 10000" : $"{p.Name} [{p.Article}] stock {p.Stock}";
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
}
