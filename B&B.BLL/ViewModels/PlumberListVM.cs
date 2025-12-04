using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class PlumberListVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public List<PlumberReceiptVM> Receipts { get; set; } = new();
    }
    public class PlumberReceiptVM
    {
        public int ReceiptId { get; set; }
        public DateTime Date { get; set; }
        public string? ClientName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }
    }

}
