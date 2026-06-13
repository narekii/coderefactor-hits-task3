using AutoServiceApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceApp.Services
{
    public class PricingService
    {
        public decimal CalculateOrderCost(RepairOrder order, bool final, PaymentType paymentMethod)
        {
            var works = order.Works.Sum(x => x.Cost + (decimal)x.Hours * (order.AssignedMechanic?.HourRate ?? 0));
            var parts = order.UsedPartIds.Select(id => Parts.FirstOrDefault(p => p.Id == id)).Where(p => p != null).Sum(p => p!.Price * 1.20m);
            var result = works + parts;
            decimal tempDiscount = 0;
            if (paymentMethod == PaymentType.Card)
                result += result * 0.05m;
            if (order.Customer != null && order.Customer.Cars.Count > 2)
                result -= result * 0.10m;
            if (final && order.Status == OrderStatus.Ready)
                result += 500;
            if (result > 10000)
                tempDiscount = result * 0.15m;
            else
                tempDiscount = 0;
            return result - tempDiscount;
        }
    }
}
