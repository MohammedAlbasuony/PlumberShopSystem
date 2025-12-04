using B_B.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.ViewModels
{
    public class DashboardVM
    {
        // Financial
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalRefunds { get; set; }
        public int PaidReceiptsCount { get; set; }
        public int UnpaidReceiptsCount { get; set; }
        public List<ReceiptSummaryVM> TopReceipts { get; set; } = new();

        // Clients & Suppliers
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public List<ClientWithReceiptsVM> TopClients { get; set; } = new();

        // Products
        public Decimal TotalProducts { get; set; }
        public Decimal LowStockProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<ProductVM> BestSellingProducts { get; set; } = new();
        public List<ProductVM> SlowProducts { get; set; } = new();

        // Activity
        public List<ReceiptSummaryVM> RecentReceipts { get; set; } = new();
    }

}
