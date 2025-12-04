using B_B.BLL.ViewModels;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace B_B.PLL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClientRepository _clientRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IReceiptRepository _receiptRepo;
        private readonly IProductRepository _productRepo;

        public HomeController(
            IClientRepository clientRepo,
            ISupplierRepository supplierRepo,
            IReceiptRepository receiptRepo,
            IProductRepository productRepo)
        {
            _clientRepo = clientRepo;
            _supplierRepo = supplierRepo;
            _receiptRepo = receiptRepo;
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Load data
                var clients = await _clientRepo.GetAllAsync();
                var receipts = await _receiptRepo.GetAllReceiptsAsync();
                var products = await _productRepo.GetAllAsync();

                // ✅ Use totals already computed in repository
                var totalIn = receipts.Where(r => r.ReceiptType == ReceiptType.In).Sum(r => r.TotalAmount);
                var totalOut = receipts.Where(r => r.ReceiptType == ReceiptType.Out).Sum(r => r.TotalAmount);
                var totalRefunds = receipts.Sum(r => r.RefundAmount);
                var balance = totalIn - totalOut;

                var paidCount = receipts.Count(r => r.RemainingAmount == 0);
                var unpaidCount = receipts.Count(r => r.RemainingAmount != 0);

                var vm = new DashboardVM
                {
                    // ✅ Clients
                    TotalClients = clients.Count(),
                    ActiveClients = clients.Count(c =>
                        c.Receipts != null && c.Receipts.Any(r => r.Date >= DateTime.Now.AddDays(-30))),

                    // ✅ Products
                    TotalProducts = products.Count(),
                    LowStockProducts = products.Count(p => p.Quantity < 10),
                    TotalStockValue = products.Sum(p => p.Quantity * p.Cost),

                    // ✅ Financial overview
                    TotalIn = totalIn,
                    TotalOut = totalOut,
                    Balance = balance,
                    TotalRefunds = totalRefunds,
                    PaidReceiptsCount = paidCount,
                    UnpaidReceiptsCount = unpaidCount,

                    // ✅ Recent receipts
                    RecentReceipts = receipts
                        .OrderByDescending(r => r.Date)
                        .Take(6)
                        .Select(r => new ReceiptSummaryVM
                        {
                            ReceiptId = r.Id,
                            Date = r.Date,
                            TotalAmount = r.TotalAmount,
                            RefundAmount = r.RefundAmount,
                            PaidAmount = r.PaidAmount,
                            ReceiptType = r.ReceiptType == ReceiptType.In ? "In" : "Out"
                        })
                        .ToList(),

                    // ✅ Top receipts
                    TopReceipts = receipts
                        .OrderByDescending(r => r.TotalAmount)
                        .Take(5)
                        .Select(r => new ReceiptSummaryVM
                        {
                            ReceiptId = r.Id,
                            Date = r.Date,
                            TotalAmount = r.TotalAmount,
                            RefundAmount = r.RefundAmount,
                            PaidAmount = r.PaidAmount,
                            ReceiptType = r.ReceiptType == ReceiptType.In ? "In" : "Out"
                        })
                        .ToList(),

                    // ✅ Top clients by purchase total
                    TopClients = clients
                        .Where(c => c.Receipts != null && c.Receipts.Any())
                        .OrderByDescending(c => c.Receipts.Sum(r => r.TotalAmount))
                        .Take(5)
                        .Select(c => new ClientWithReceiptsVM
                        {
                            ClientId = c.Id,
                            Name = c.Name,
                            Receipts = c.Receipts
                                .OrderByDescending(r => r.Date)
                                .Take(3)
                                .Select(r => new ReceiptSummaryVM
                                {
                                    ReceiptId = r.Id,
                                    Date = r.Date,
                                    TotalAmount = r.TotalAmount,
                                    RefundAmount = r.RefundAmount,
                                    PaidAmount = r.PaidAmount,
                                    ReceiptType = r.ReceiptType == ReceiptType.In ? "In" : "Out"
                                })
                                .ToList()
                        })
                        .ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات. يرجى المحاولة مرة أخرى.";
                Console.WriteLine(ex); // optional: for debug in console/log
                return View(new DashboardVM());
            }
        }



        public IActionResult Error()
        {
            return View();
        }
    }
}