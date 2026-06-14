using B_B.BLL.ViewModels;
using B_B.DAL.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
                    .ThenInclude(r => r.ReceiptDetails)
                            .ThenInclude(d => d.Product)
                .ToListAsync();

            var vm = plumbers.Select(p => new PlumberListVM
            {
                Id = p.Id,
                Name = p.Name,
                Phone = p.Phone,

                ReceiptCount = p.OutReceipts.Count,

                TotalAmount = p.OutReceipts.Sum(r =>
                    r.ReceiptDetails.Sum(d =>
                        (d.Product?.Cost ?? 0) * (d.Quantity - d.RefundQuantity)))
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
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plumber == null)
                return NotFound();

            // All products used by this plumber in all receipts
            var allReceiptLines = plumber.OutReceipts
                .SelectMany(r => r.ReceiptDetails)
                .Where(d => d.Product != null)
                .ToList();

            var vm = new PlumberListVM
            {
                Id = plumber.Id,
                Name = plumber.Name,
                Phone = plumber.Phone,

                // Number of receipts worked on
                ReceiptCount = plumber.OutReceipts.Count,

                // Total cost of all receipts combined
                TotalAmount = allReceiptLines.Sum(d =>
                    d.Product.Cost * (d.Quantity - d.RefundQuantity)),

                // Global supplier totals across ALL receipts
                SupplierTotals = allReceiptLines
                    .Where(d => d.Product.Supplier != null)
                    .GroupBy(d => new
                    {
                        SupplierId = d.Product.Supplier.Id,
                        SupplierName = d.Product.Supplier.Name
                    })
                    .Select(g => new SupplierTotalVM
                    {
                        SupplierName = g.Key.SupplierName,

                        // Number of receipts containing products from this supplier
                        ReceiptCount = g.Select(x => x.ReceiptId)
                                        .Distinct()
                                        .Count(),

                        // Total supplier cost across all receipts
                        TotalAmount = g.Sum(x =>
                            x.Product.Cost *
                            (x.Quantity - x.RefundQuantity))
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList(),

                // Receipt details
                Receipts = plumber.OutReceipts
                    .Select(r => new PlumberReceiptVM
                    {
                        ReceiptId = r.Id,
                        Date = r.Date,
                        ClientName = r.Client?.Name,

                        // Total cost of this receipt
                        TotalAmount = r.ReceiptDetails.Sum(d =>
                            d.Product.Cost *
                            (d.Quantity - d.RefundQuantity)),

                        // Total selling price of this receipt
                        TotalPrice = r.ReceiptDetails.Sum(d =>
                            d.UnitPrice *
                            (d.Quantity - d.RefundQuantity)),

                        PaidAmount = r.PaidAmount,
                        Status = r.Status.ToString(),

                        // Supplier breakdown INSIDE this receipt
                        SupplierTotals = r.ReceiptDetails
                            .Where(d => d.Product?.Supplier != null)
                            .GroupBy(d => new
                            {
                                SupplierId = d.Product.Supplier.Id,
                                SupplierName = d.Product.Supplier.Name
                            })
                            .Select(g => new ReceiptSupplierTotalVM
                            {
                                SupplierName = g.Key.SupplierName,

                                // Cost from this supplier in this receipt only
                                TotalCost = g.Sum(x =>
                                    x.Product.Cost *
                                    (x.Quantity - x.RefundQuantity)),

                                // Selling value from this supplier in this receipt only
                                TotalPrice = g.Sum(x =>
                                    x.UnitPrice *
                                    (x.Quantity - x.RefundQuantity))
                            })
                            .OrderByDescending(x => x.TotalCost)
                            .ToList()
                    })
                    .OrderByDescending(x => x.Date)
                    .ToList()
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> ImportSuppliers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);

            if (package.Workbook.Worksheets.Count == 0)
                return BadRequest("Uploaded file contains no worksheets.");

            var sheet = package.Workbook.Worksheets[0];
            if (sheet?.Dimension == null)
                return BadRequest("Worksheet is empty.");
            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string productName = sheet.Cells[row, 1].Text.Trim();
                string supplierName = sheet.Cells[row, 2].Text.Trim();

                if (string.IsNullOrEmpty(productName) ||
                    string.IsNullOrEmpty(supplierName))
                    continue;

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == productName);

                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.Name == supplierName);

                if (product != null && supplier != null)
                {
                    product.SupplierId = supplier.Id;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
