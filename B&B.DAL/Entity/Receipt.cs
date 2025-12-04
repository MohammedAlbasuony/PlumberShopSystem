using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Entity
{
    public class Receipt
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public ReceiptType ReceiptType { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }
        public int? PlumberId { get; set; }
        public Plumber? Plumber { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public string? CreatedBy { get; set; }
        public bool IsOld { get; set; } = false;

        public ICollection<ReceiptDetail>? ReceiptDetails { get; set; }
        // Accounting fields
        public decimal TotalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;
        public ReceiptStatus Status { get; set; } = ReceiptStatus.Draft;
    }
    public enum ReceiptStatus
    {
        Draft = 0,   // Created but not finalized
        Approved = 1,
        Cancelled = 2
    }
    public enum ReceiptType
    {
        In = 0,  // Purchase from Supplier
        Out = 1  // Sale to Client
    }
}
