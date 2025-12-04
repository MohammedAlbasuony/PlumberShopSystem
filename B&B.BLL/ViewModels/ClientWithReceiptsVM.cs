using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class ClientWithReceiptsVM
    {
        public int ClientId { get; set; }
        public string Name { get; set; }

        public List<ReceiptSummaryVM> Receipts { get; set; } = new();
    }

    public class ReceiptSummaryVM
    {
        public int ReceiptId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }   // before refund
        public decimal RefundAmount { get; set; }  // refund only
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => AdjustedTotal - PaidAmount;
        public string ReceiptType { get; set; } = string.Empty;


        // ✅ Total after subtracting refunds
        public decimal AdjustedTotal => TotalAmount - RefundAmount;
    }



}
