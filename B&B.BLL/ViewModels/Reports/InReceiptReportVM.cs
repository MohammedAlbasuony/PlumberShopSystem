using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels.Reports
{
    public class InReceiptReportVM
    {
        public int Id { get; set; }
        public string SupplierName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }
    public class ReportSummaryVM
    {
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining => TotalAmount - TotalPaid;
        public decimal TotalRefund { get; set; } // ✅ NEW

        public string TopPartyName { get; set; }  // Supplier or Client
        public decimal TopPartyAmount { get; set; }

        public IEnumerable<ReceiptDetailsVM> Receipts { get; set; }
    }

    // Receipt details for list section
    public class ReceiptDetailsVM
    {
        public int Id { get; set; }
        public string PartyName { get; set; } // Supplier for In, Client for Out
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RefundAmount { get; set; } // ✅ NEW

    }
    public class OutReceiptReportVM
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
