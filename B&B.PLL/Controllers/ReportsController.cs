using B_B.BLL.Service.Abstraction;
using B_B.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace B_B.PLL.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: Reports/InReceipts
        public async Task<IActionResult> InReceipts(int year = 0)
        {
            var vm = await _reportService.GetInReceiptsReportAsync(year);
            return View(vm);
        }

        public async Task<IActionResult> OutReceipts(int year = 0)
        {
            var vm = await _reportService.GetOutReceiptsReportAsync(year);
            return View(vm);
        }




        [HttpGet]
        public async Task<IActionResult> PrintFilteredReport(
     string? client = null,
     string? amount = null,
     string? remaining = null, // ✅ Added remaining filter
     DateTime? from = null,
     DateTime? to = null)
        {
            var reportSummary = await _reportService.GetOutReceiptsReportAsync();

            // 🔹 Apply filters (matching client-side filters)
            var filtered = reportSummary.Receipts.AsQueryable();

            if (!string.IsNullOrEmpty(client) && client != "all")
                filtered = filtered.Where(r => r.PartyName == client);

            if (!string.IsNullOrEmpty(amount) && amount != "all")
            {
                filtered = amount switch
                {
                    "0-1000" => filtered.Where(r => r.TotalAmount < 1000),
                    "1000-5000" => filtered.Where(r => r.TotalAmount >= 1000 && r.TotalAmount < 5000),
                    "5000-10000" => filtered.Where(r => r.TotalAmount >= 5000 && r.TotalAmount < 10000),
                    "10000+" => filtered.Where(r => r.TotalAmount >= 10000),
                    _ => filtered
                };
            }

            if (from.HasValue)
                filtered = filtered.Where(r => r.Date >= from.Value);

            if (to.HasValue)
                filtered = filtered.Where(r => r.Date <= to.Value);

            // ✅ Apply the remaining money filter
            if (!string.IsNullOrEmpty(remaining) && remaining == "withRemaining")
                filtered = filtered.Where(r => (r.TotalAmount - r.PaidAmount) > 0);

            var list = filtered
                .OrderByDescending(r => r.Date)
                .Select(r => new ReportReceiptVM
                {
                    Id = r.Id,
                    PartyName = r.PartyName,
                    Date = r.Date,
                    TotalAmount = r.TotalAmount,
                    PaidAmount = r.PaidAmount,
                    RefundAmount = r.RefundAmount,
                    Remaining = r.TotalAmount - r.PaidAmount
                })
                .ToList();

            var vm = new FilteredReportVM
            {
                ClientName = client == "all" || string.IsNullOrEmpty(client) ? "جميع العملاء" : client,
                FromDate = from,
                ToDate = to,
                AmountFilter = amount ?? "الكل",
                Receipts = list,
                TotalAmount = list.Sum(x => x.TotalAmount),
                TotalRefund = list.Sum(x => x.RefundAmount),
                TotalPaid = list.Sum(x => x.PaidAmount),
                TotalRemaining = list.Sum(x => x.TotalAmount - x.PaidAmount)
            };

            return View("PrintFilteredReport", vm);
        }

    }
}
