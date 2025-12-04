using B_B.BLL.Service.Abstraction;
using B_B.BLL.Service.Implementation;
using B_B.BLL.ViewModels;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace B_B.PLL.Controllers
{
    public class SupplierController : Controller
    {
        private readonly SupplierService _supplierService;
        private readonly ISupplierRepository _supplierRepo;

        public SupplierController(SupplierService supplierService, ISupplierRepository supplierRepo)
        {
            _supplierService = supplierService;
            _supplierRepo = supplierRepo;
        }

        // 🔹 List all suppliers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = (await _supplierRepo.GetAllAsync())
                .OrderByDescending(s => s.Id)
                .ToList();

            var vm = new List<SupplierWithReceiptsVM>();

            foreach (var supplier in suppliers)
            {
                var fullSupplier = await _supplierRepo.GetSupplierWithReceiptsAsync(supplier.Id);

                if (fullSupplier == null) continue;

                vm.Add(new SupplierWithReceiptsVM
                {
                    SupplierId = fullSupplier.Id,
                    Name = fullSupplier.Name,
                    Receipts = fullSupplier.Receipts
                        .OrderByDescending(r => r.Date)
                        .Select(r => new ReceiptSummaryVM
                        {
                            ReceiptId = r.Id,
                            Date = r.Date,
                            TotalAmount = (r.ReceiptDetails != null && r.ReceiptDetails.Any())
                            ? r.ReceiptDetails.Sum(d => d.UnitPrice * d.Quantity)
                            : r.TotalAmount,
                            RefundAmount = r.ReceiptDetails?.Sum(d => d.UnitPrice * d.RefundQuantity) ?? 0,
                            PaidAmount = r.PaidAmount
                        })
                        .ToList()
                });
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(int receiptId, decimal amount)
        {
            try
            {
                await _supplierRepo.AddPaymentAsync(
                    receiptId,
                    amount,
                    User.Identity?.Name ?? "النظام"
                );

                return Json(new { success = true, message = "تمت إضافة الدفعة للفاتورة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddPaymentToSupplier(int supplierId, decimal amount)
        {
            try
            {
                await _supplierRepo.AddPaymentToSupplierAsync(
                    supplierId,
                    amount,
                    User.Identity?.Name ?? "النظام"
                );

                return Json(new { success = true, message = "تمت إضافة الدفعة للمورد بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }


        // 🔹 Supplier details (with receipts)
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierService.GetSupplierDetailsAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // 🔹 Create (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                await _supplierRepo.AddAsync(supplier);
                await _supplierRepo.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // 🔹 Edit (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // 🔹 Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _supplierRepo.Update(supplier);
                await _supplierRepo.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null)
                return Json(new { success = false, message = "Supplier not found" });

            _supplierRepo.Delete(supplier);
            await _supplierRepo.SaveAsync();

            return Json(new { success = true, message = "Supplier deleted successfully" });
        }

    }

}
