using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class ReceiptListVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ReceiptType { get; set; } = string.Empty;
        public string ReceiptStatus { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public string? ClientName { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string? PlumberName { get; set; }
        public decimal? CostAmount { get; set; }
        public decimal? Profit { get; set; }
        public List<ReceiptDetailVM> ReceiptDetails { get; set; } = new();
        public List<SupplierSummaryVM> SupplierSummaries { get; set; } = new();


    }
    public class SupplierSummaryVM
    {
        public string SupplierName { get; set; } = "";
        public decimal TotalBeforeDiscount { get; set; }
        public decimal RefundBeforeDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalAmount { get; set; } // after discount
        public decimal RefundAmount { get; set; } // after discount
        public decimal NetAmount => TotalAmount - RefundAmount;
    }

}
