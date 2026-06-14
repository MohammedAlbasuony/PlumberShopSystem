using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class SupplierTotalVM
    {
        public string SupplierName { get; set; }

        public decimal TotalAmount { get; set; }

        public int ReceiptCount { get; set; }
    }
}
