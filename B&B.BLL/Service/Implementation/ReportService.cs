using B_B.BLL.Service.Abstraction;
using B_B.BLL.ViewModels.Reports;
using B_B.DAL.DB;
using B_B.DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace B_B.BLL.Service.Implementation
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDBcontext _context;

        public ReportService(ApplicationDBcontext context)
        {
            _context = context;
        }



        public async Task<ReportSummaryVM> GetInReceiptsReportAsync(int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;

            var receipts = await _context.Receipts
                .Include(r => r.Supplier)
                .Where(r => r.ReceiptType == ReceiptType.In && r.Date.Year == year)
                .ToListAsync();

            if (!receipts.Any())
                return new ReportSummaryVM { Receipts = new List<ReceiptDetailsVM>() };

            var grouped = receipts
                .GroupBy(r => r.Supplier.Name)
                .Select(g => new { Supplier = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();

            return new ReportSummaryVM
            {
                TotalAmount = receipts.Sum(r => r.TotalAmount),
                TotalCount = receipts.Count,
                TotalPaid = receipts.Sum(r => r.PaidAmount),
                TopPartyName = grouped?.Supplier,
                TopPartyAmount = grouped?.Total ?? 0,
                Receipts = receipts
                    .OrderByDescending(r => r.Date) // ✅ Order by newest first
                    .Select(r => new ReceiptDetailsVM
                    {
                        Id = r.Id,
                        PartyName = r.Supplier.Name,
                        Date = r.Date,
                        TotalAmount = r.TotalAmount,
                        PaidAmount = r.PaidAmount
                    })
                    .ToList()
            };
        }
        public async Task<ReportSummaryVM> GetOutReceiptsReportAsync(int year = 0)
        {
            if (year == 0)
                year = DateTime.Now.Year;

            var receipts = await _context.Receipts
                .Include(r => r.Client)
                .Where(r => r.ReceiptType == ReceiptType.Out && r.Date.Year == year)
                .ToListAsync();

            if (!receipts.Any())
                return new ReportSummaryVM { Receipts = new List<ReceiptDetailsVM>() };

            var grouped = receipts
                .GroupBy(r => r.Client.Name)
                .Select(g => new { Client = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();

            return new ReportSummaryVM
            {
                TotalAmount = receipts.Sum(r => r.TotalAmount),
                TotalPaid = receipts.Sum(r => r.PaidAmount),
                TotalRefund = receipts.Sum(r => r.RefundAmount),
                TotalCount = receipts.Count,
                TopPartyName = grouped?.Client,
                TopPartyAmount = grouped?.Total ?? 0,
                Receipts = receipts
                    .OrderByDescending(r => r.Date) // ✅ Order by newest first
                    .Select(r => new ReceiptDetailsVM
                    {
                        Id = r.Id,
                        PartyName = r.Client.Name,
                        Date = r.Date,
                        TotalAmount = r.TotalAmount,
                        PaidAmount = r.PaidAmount,
                        RefundAmount = r.RefundAmount
                    })
                    .ToList()
            };
        }



    }
}
