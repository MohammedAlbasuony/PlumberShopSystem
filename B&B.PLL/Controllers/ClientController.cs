using B_B.BLL.Service.Abstraction;
using B_B.BLL.Service.Implementation;
using B_B.BLL.ViewModels;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace B_B.PLL.Controllers
{
    public class ClientController : Controller
    {
        private readonly ClientService _clientService;
        private readonly IClientRepository _clientRepo;
        private readonly IReceiptService _receiptService;

        public ClientController(ClientService clientService, IClientRepository clientRepo , IReceiptService receiptService)
        {
            _clientService = clientService;
            _clientRepo = clientRepo;
            _receiptService = receiptService;
        }

        // 🔹 List all clients
        public async Task<IActionResult> Index()
        {
            var clients = (await _clientRepo.GetAllAsync())
                .OrderByDescending(c => c.Id)
                .ToList();

            var vm = new List<ClientWithReceiptsVM>();

            foreach (var client in clients)
            {
                var fullClient = await _clientRepo.GetClientWithReceiptsAsync(client.Id);
                if (fullClient == null) continue;

                vm.Add(new ClientWithReceiptsVM
                {
                    ClientId = fullClient.Id,
                    Name = fullClient.Name,
                    Receipts = fullClient.Receipts
                        .OrderByDescending(r => r.Date)
                        .Select(r => new ReceiptSummaryVM
                        {
                            ReceiptId = r.Id,
                            Date = r.Date,
                            ReceiptType = r.ReceiptType.ToString(),

                            // ✅ Total amount AFTER discount but BEFORE refund
                            TotalAmount = r.IsOld
                                ? r.TotalAmount
                                : (r.ReceiptDetails?.Sum(d =>
                                    (d.UnitPrice * d.Quantity) *
                                    (1 - (d.DiscountPercentage / 100m))) ?? 0m),

                            // ✅ Refund amount AFTER discount
                            RefundAmount = r.IsOld
                                ? r.RefundAmount
                                : (r.ReceiptDetails?.Sum(d =>
                                    (d.UnitPrice * d.RefundQuantity) *
                                    (1 - (d.DiscountPercentage / 100m))) ?? 0m),

                            // ✅ Paid amount
                            PaidAmount = r.Payments?.Sum(p => p.Amount) ?? r.PaidAmount
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
                await _clientService.AddPaymentAsync(receiptId, amount);

                return Json(new
                {
                    success = true,
                    message = "تمت إضافة الدفعة بنجاح"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء إضافة الدفعة: " + ex.Message
                });
            }
        }



        // 🔹 Client details (with receipts)
        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetClientDetailsAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // 🔹 Create (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 Create (POST)
        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientRepo.AddAsync(client);
                await _clientRepo.SaveAsync();
                TempData["Success"] = "Client added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

       

        // 🔹 Edit (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                _clientRepo.Update(client);
                await _clientRepo.SaveAsync();
                TempData["Success"] = "Client updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // 🔹 Delete (AJAX + SweetAlert)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await _clientRepo.GetByIdAsync(id);
                if (client == null)
                    return Json(new { success = false, message = "العميل غير موجود" });

                if (client.Receipts != null && client.Receipts.Any())
                    return Json(new { success = false, message = "لا يمكن حذف العميل لأنه لديه فواتير مرتبطة" });

                 _clientRepo.Delete(client);
                 await _clientRepo.SaveAsync();
                return Json(new { success = true, message = "تم حذف العميل بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ أثناء الحذف: " + ex.Message });
            }
        }

    }

}
