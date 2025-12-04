using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class SupplierWithReceiptsVM
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public List<ReceiptSummaryVM> Receipts { get; set; } = new();
    }
}
