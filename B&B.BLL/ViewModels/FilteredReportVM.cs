using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class FilteredReportVM
    {
        public string ClientName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string AmountFilter { get; set; }
        public List<ReportReceiptVM> Receipts { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalRefund { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
    }

    public class ReportReceiptVM
    {
        public int Id { get; set; }
        public string PartyName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Remaining { get; set; }
    }

}
