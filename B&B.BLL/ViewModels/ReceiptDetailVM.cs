using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class ReceiptDetailVM
    {
        [Required]
        public int ProductId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Quantity must be at least 0.01")]
        public decimal Quantity { get; set; }

        public decimal Cost { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; } = string.Empty; // ✅ required
        public decimal? RefundQuantity { get; set; } // 🔹 New
        public decimal DiscountPercentage { get; set; } 

        public decimal Total =>
                   (Quantity * UnitPrice) * (1 - (DiscountPercentage / 100));
    }


}
