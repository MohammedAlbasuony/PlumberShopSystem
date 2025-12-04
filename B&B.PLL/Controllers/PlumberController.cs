using B_B.BLL.ViewModels;
using B_B.DAL.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B_B.PLL.Controllers
{
    public class PlumberController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public PlumberController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // ✅ Show all plumbers and their receipts
        public async Task<IActionResult> Index()
        {
            var plumbers = await _context.Plumbers
                .Include(p => p.OutReceipts)
                    .ThenInclude(r => r.Client)
                .Include(p => p.OutReceipts)
                    .ThenInclude(r => r.ReceiptDetails)
                .ToListAsync();

            var vm = plumbers.Select(p => new PlumberListVM
            {
                Id = p.Id,
                Name = p.Name,
                Phone = p.Phone,
                Receipts = p.OutReceipts.Select(r => new PlumberReceiptVM
                {
                    ReceiptId = r.Id,
                    Date = r.Date,
                    ClientName = r.Client?.Name,
                    TotalAmount = r.ReceiptDetails?.Sum(d => d.UnitPrice * (d.Quantity - d.RefundQuantity)) ?? 0,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status.ToString()
                }).ToList()
            }).ToList();

            return View(vm);
        }


        // ✅ Details for a specific plumber
        public async Task<IActionResult> Details(int id)
        {
            var plumber = await _context.Plumbers
                .Include(p => p.OutReceipts)
                    .ThenInclude(r => r.Client)
                .Include(p => p.OutReceipts)
                 .ThenInclude(r => r.ReceiptDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plumber == null)
                return NotFound();

            var vm = new PlumberListVM
            {
                Id = plumber.Id,
                Name = plumber.Name,
                Phone = plumber.Phone,
                Receipts = plumber.OutReceipts.Select(r => new PlumberReceiptVM
                {
                    ReceiptId = r.Id,
                    Date = r.Date,
                    ClientName = r.Client?.Name,
                    TotalAmount = r.ReceiptDetails.Sum(d => d.UnitPrice * (d.Quantity - d.RefundQuantity)),
                    PaidAmount = r.PaidAmount,
                    Status = r.Status.ToString()
                }).ToList()
            };

            return View(vm);
        }
    }
}
