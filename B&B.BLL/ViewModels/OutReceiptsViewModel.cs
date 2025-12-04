using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class OutReceiptsViewModel
    {
        public List<ReceiptListVM> ApprovedReceipts { get; set; }
        public List<ReceiptListVM> DraftReceipts { get; set; }
    }

}
