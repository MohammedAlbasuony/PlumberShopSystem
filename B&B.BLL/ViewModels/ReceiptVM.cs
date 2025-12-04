using B_B.DAL.Entity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace B_B.BLL.ViewModels
{
    public class ReceiptVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public ReceiptType ReceiptType { get; set; }

        public int? SupplierId { get; set; }
        public int? ClientId { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RefundAmount { get; set; }
        // New Supplier
        public string? NewSupplierName { get; set; }
        public string? NewSupplierPhone { get; set; }
        public List<ReceiptDetailVM> ReceiptDetails { get; set; } = new();

        // Dropdown lists
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
        public IEnumerable<SelectListItem>? Clients { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }



       

    }

}
