using System.Text;
using AutoServiceApp.Models;

namespace AutoServiceApp.Services;
public record DateRange(DateTime From, DateTime To);

public class ReportService
{
    private const int BonusOrderThreshold = 5;
    private const int MechanicBonusAmount = 1000;
    private const int LowStockThreshold = 3;
    private const string RestockMessage = "reorder at least 10000";
    public string BuildReports(List<RepairOrder> orders, List<Mechanic> mechanics, List<Part> parts, DateRange period)
    {
        return BuildRevenueReport(orders, period) + "\n"
            + BuildPopularWorks(orders) + "\n\n"
            + BuildMechanicsLoad(mechanics, orders) + "\n"
            + BuildPartsStock(parts);
    }
    public string BuildRevenueReport(List<RepairOrder> orders, DateRange period)
    {
        var result = new StringBuilder();
        var selected = orders.Where(o => o.AcceptedAt.Date >= period.From.Date && o.AcceptedAt.Date <= period.To.Date).ToList();
        result.AppendLine($"Revenue for period {period.From:d} - {period.To:d}: {selected.Sum(x => x.Cost):C}");
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
            var count = orders.Count(o => o.AssignedMechanicId == m.Id && o.Status != OrderStatus.Released);
            var bonus = count > BonusOrderThreshold ? MechanicBonusAmount : 0;
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
            var line = p.Stock < LowStockThreshold ? $"{p.Name} [{p.Article}] stock {p.Stock}, {RestockMessage}" : $"{p.Name} [{p.Article}] stock {p.Stock}";
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
}
