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
        public async Task<IActionResult> PrintFilteredReport(string? ids = null)
        {
            var reportSummary = await _reportService.GetOutReceiptsReportAsync();

            var receipts = reportSummary.Receipts.AsQueryable();

            if (!string.IsNullOrEmpty(ids))
            {
                var idList = ids.Split(',')
                                .Select(int.Parse)
                                .ToList();

                receipts = receipts.Where(r => idList.Contains(r.Id));
            }

            var list = receipts
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
                Receipts = list,
                TotalAmount = list.Sum(x => x.TotalAmount),
                TotalRefund = list.Sum(x => x.RefundAmount),
                TotalPaid = list.Sum(x => x.PaidAmount),
                TotalRemaining = list.Sum(x => x.Remaining)
            };

            return View("PrintFilteredReport", vm);
        }

    }
}
