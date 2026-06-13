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
        private const decimal PartsMarkupRate = 1.20m;
        private const decimal CardPaymentFeeRate = 0.05m;
        private const decimal VipDiscountRate = 0.10m;
        private const decimal BulkDiscountThreshold = 10000m;
        private const decimal BulkDiscountRate = 0.15m;
        public decimal CalculateOrderCost(RepairOrder order, bool final, PaymentType paymentMethod, List<Part> Parts)
        {
            var works = order.Works.Sum(x => x.Cost + (decimal)x.Hours * (order.AssignedMechanic?.HourRate ?? 0));
            var parts = order.UsedPartIds.Select(id => Parts.FirstOrDefault(p => p.Id == id)).Where(p => p != null).Sum(p => p!.Price * PartsMarkupRate);
            var result = works + parts;
            decimal tempDiscount = 0;
            if (paymentMethod == PaymentType.Card)
                result += result * CardPaymentFeeRate;
            if (order.Customer != null && order.Customer.Cars.Count > 2)
                result -= result * VipDiscountRate;
            if (final && order.Status == OrderStatus.Ready)
                result += 500;
            if (result > BulkDiscountThreshold)
                tempDiscount = result * BulkDiscountRate;
            else
                tempDiscount = 0;
            return result - tempDiscount;
        }
    }
}
