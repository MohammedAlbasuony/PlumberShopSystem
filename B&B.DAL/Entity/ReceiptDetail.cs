using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Entity
{
    public class ReceiptDetail
    {
        public int Id { get; set; }

        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RefundQuantity { get; set; } = 0;

        // ✅ New: Discount percentage per product
        public decimal DiscountPercentage { get; set; } = 0;

        // ✅ Total after discount (replaces old Total logic)
        public decimal Total =>
            (Quantity * UnitPrice) * (1 - (DiscountPercentage / 100));

        public decimal CostPrice { get; set; }
    }
}
