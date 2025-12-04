using B_B.DAL.Entity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class EditReceiptVM
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public ReceiptType ReceiptType { get; set; }   // In or Out

        // Relations
        public int? SupplierId { get; set; }
        public int? ClientId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be >= 0")]
        public decimal PaidAmount { get; set; }
        public decimal RefundAmount { get; set; }

        public List<ReceiptDetailVM> ReceiptDetails { get; set; } = new();

        // Dropdowns
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
        public IEnumerable<SelectListItem>? Clients { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }
    }
}
