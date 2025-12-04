using B_B.BLL.Service.Abstraction;
using B_B.BLL.Service.Implementation;
using B_B.BLL.ViewModels;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace B_B.PLL.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly IReceiptService _receiptService;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IProductRepository _productRepo;
        private readonly IGenericRepository<Plumber> _plumberRepo;



        public ReceiptController(
            IReceiptService receiptService,
            ISupplierRepository supplierRepo,
            IClientRepository clientRepo,
            IProductRepository productRepo,
            IGenericRepository<Plumber> plumberRepo)
        {
            _receiptService = receiptService;
            _supplierRepo = supplierRepo;
            _clientRepo = clientRepo;
            _productRepo = productRepo;
            _plumberRepo = plumberRepo;
        }
        const string InReceiptDraftKey = "IN_RECEIPT_DRAFT";
        const string OutReceiptDraftKey = "OUT_RECEIPT_DRAFT";

        // -------------------- INDEX --------------------

        [HttpGet]
        public async Task<IActionResult> InReceipts()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();

            var vm = receipts
                .Where(r => r.ReceiptType == ReceiptType.In)
                .OrderByDescending(r => r.Date)   // 🔹 newest first
                .Select(r => new ReceiptListVM
                {
                    Id = r.Id,
                    Date = r.Date,
                    ReceiptType = r.ReceiptType.ToString(),
                    SupplierName = r.Supplier?.Name,
                    ClientName = r.Client?.Name,
                    Total = r.TotalAmount,
                    PaidAmount = r.PaidAmount,
                    RefundAmount = r.RefundAmount,
                    RemainingAmount = r.RemainingAmount
                })
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> OutReceipts()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();

            var vm = receipts
                .Where(r => r.ReceiptType == ReceiptType.Out)
                .OrderByDescending(r => r.Date) // newest first
                .Select(r => new ReceiptListVM
                {
                    Id = r.Id,
                    Date = r.Date,
                    ReceiptType = r.ReceiptType.ToString(),
                    SupplierName = r.Supplier?.Name,
                    ClientName = r.Client?.Name,
                    Total = r.TotalAmount,
                    PaidAmount = r.PaidAmount,
                    RefundAmount = r.RefundAmount,
                    RemainingAmount = r.RemainingAmount,
                    ReceiptStatus = r.Status.ToString(),

                    // Calculate total cost (sum of product cost * quantity sold)
                    CostAmount = r.ReceiptDetails.Sum(d =>
                        (d.Product?.Cost ?? 0) * (d.Quantity - d.RefundQuantity)),

                    // Profit = Total sold - total cost
                    Profit = r.TotalAmount - r.ReceiptDetails.Sum(d =>
                        (d.Product?.Cost ?? 0) * (d.Quantity - d.RefundQuantity))
                })
                .ToList();

            return View(vm);
        }



        // -------------------- IN RECEIPT --------------------
        [HttpPost]
        public IActionResult SaveInReceiptDraft([FromBody] ReceiptVM vm)
        {
            HttpContext.Session.SetString(InReceiptDraftKey, JsonSerializer.Serialize(vm));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CreateInReceipt()
        {
            ReceiptVM vm = null;

            if (HttpContext.Session.TryGetValue(InReceiptDraftKey, out var draftBytes))
            {
                var json = System.Text.Encoding.UTF8.GetString(draftBytes);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    vm = JsonSerializer.Deserialize<ReceiptVM>(json);
                }
            }

            if (vm == null)
            {
                vm = await BuildInReceiptVM();
            }

            vm.ReceiptDetails ??= new List<ReceiptDetailVM>();

            // Enhance product info safely (just like OutReceipt)
            await EnhanceReceiptDetailsWithProductInfo(vm);

            await PopulateInLookups(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInReceipt(ReceiptVM vm)
        {
            if (!ModelState.IsValid)
            {
                await EnhanceReceiptDetailsWithProductInfo(vm);
                HttpContext.Session.SetString(InReceiptDraftKey, JsonSerializer.Serialize(vm));
                await PopulateInLookups(vm);
                return View(vm);
            }

            Supplier? newSupplier = null;
            if (!vm.SupplierId.HasValue && !string.IsNullOrWhiteSpace(vm.NewSupplierName))
            {
                newSupplier = new Supplier
                {
                    Name = vm.NewSupplierName,
                    Phone = vm.NewSupplierPhone
                };
            }
            else if (!vm.SupplierId.HasValue)
            {
                ModelState.AddModelError("SupplierId", "الرجاء اختيار أو إضافة مورد.");
                await EnhanceReceiptDetailsWithProductInfo(vm);
                HttpContext.Session.SetString(InReceiptDraftKey, JsonSerializer.Serialize(vm));
                await PopulateInLookups(vm);
                return View(vm);
            }

            var receipt = new Receipt
            {
                Date = vm.Date,
                ReceiptType = ReceiptType.In,
                PaidAmount = vm.PaidAmount,
                RefundAmount = vm.RefundAmount,
                SupplierId = vm.SupplierId,
                ReceiptDetails = vm.ReceiptDetails.Select(d => new ReceiptDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            await _receiptService.AddInReceiptAsync(receipt, newSupplier);

            // Remove draft only on success
            HttpContext.Session.Remove(InReceiptDraftKey);

            return RedirectToAction("InReceipts");
        }
        private async Task EnhanceReceiptDetailsWithProductInfo(ReceiptVM vm)
        {
            if (vm == null || vm.ReceiptDetails == null || !vm.ReceiptDetails.Any())
                return;

            var products = await _productRepo.GetAllAsync();

            foreach (var detail in vm.ReceiptDetails)
            {
                var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                if (product != null)
                {
                    detail.ProductName = product.Name;
                    detail.Cost = product.Cost;
                    if (detail.UnitPrice == 0)
                        detail.UnitPrice = product.SellingPrice;
                }
                else
                {
                    detail.ProductName ??= "منتج غير معروف";
                    detail.Cost = detail.Cost != 0 ? detail.Cost : 0;
                }
            }
        }
        // -------------------- OUT RECEIPT --------------------
        [HttpPost]
        public IActionResult SaveOutReceiptDraft([FromBody] OutReceiptVM vm)
        {
            HttpContext.Session.SetString(OutReceiptDraftKey, JsonSerializer.Serialize(vm));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CreateOutReceipt()
        {
            OutReceiptVM vm = null;

            if (HttpContext.Session.TryGetValue(OutReceiptDraftKey, out var draftBytes))
            {
                var json = System.Text.Encoding.UTF8.GetString(draftBytes);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    vm = JsonSerializer.Deserialize<OutReceiptVM>(json);
                }
            }

            if (vm == null)
            {
                vm = await BuildOutReceiptVM();
            }

            // 🔥 Ensure ReceiptDetails is never null
            vm.ReceiptDetails ??= new List<ReceiptDetailVM>();

            // 🔥 Enhance product info safely
            await EnhanceReceiptDetailsWithProductInfo(vm);

            await PopulateOutLookups(vm);
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOutReceipt(OutReceiptVM vm)
        {
            if (!ModelState.IsValid)
            {
                // 🔥 ENHANCE: Add product information to receipt details
                await EnhanceReceiptDetailsWithProductInfo(vm);

                // 🔥 SAVE DRAFT
                HttpContext.Session.SetString(OutReceiptDraftKey, JsonSerializer.Serialize(vm));

                await PopulateOutLookups(vm);
                return View(vm);
            }

            if (!vm.ClientId.HasValue && string.IsNullOrWhiteSpace(vm.NewClientName))
            {
                ModelState.AddModelError("ClientId", "الرجاء اختيار أو إضافة عميل.");

                // 🔥 ENHANCE: Add product information to receipt details
                await EnhanceReceiptDetailsWithProductInfo(vm);

                // 🔥 SAVE AGAIN
                HttpContext.Session.SetString(OutReceiptDraftKey, JsonSerializer.Serialize(vm));

                await PopulateOutLookups(vm);
                return View(vm);
            }

            // prepare optional client + plumber
            Client? newClient = null;
            if (!vm.ClientId.HasValue && !string.IsNullOrWhiteSpace(vm.NewClientName))
            {
                newClient = new Client
                {
                    Name = vm.NewClientName,
                    Phone = vm.NewClientPhone
                };
            }

            Plumber? newPlumber = null;
            if (!vm.PlumberId.HasValue && !string.IsNullOrWhiteSpace(vm.NewPlumberName))
            {
                newPlumber = new Plumber
                {
                    Name = vm.NewPlumberName,
                    Phone = vm.NewPlumberPhone
                };
            }

            // Build receipt
            var receipt = new Receipt
            {
                Date = vm.Date,
                ReceiptType = ReceiptType.Out,
                PaidAmount = vm.PaidAmount,
                ClientId = newClient?.Id ?? vm.ClientId,
                PlumberId = newPlumber?.Id ?? vm.PlumberId,
                RefundAmount = vm.RefundAmount,
                ReceiptDetails = vm.ReceiptDetails.Select(d => new ReceiptDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountPercentage = d.DiscountPercentage
                }).ToList()
            };

            receipt.TotalAmount = receipt.ReceiptDetails.Sum(d =>
                (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));

            var receiptId = await _receiptService.CreateOutReceiptDraftAsync(receipt, newClient, newPlumber);

            // 🔥 Remove draft only on success
            HttpContext.Session.Remove(OutReceiptDraftKey);

            return RedirectToAction("Review", new { id = receiptId });
        }

        // 🔥 ADD THIS NEW METHOD TO ENHANCE PRODUCT INFORMATION
        private async Task EnhanceReceiptDetailsWithProductInfo(OutReceiptVM vm)
        {
            if (vm == null || vm.ReceiptDetails == null || !vm.ReceiptDetails.Any())
                return;

            var products = await _productRepo.GetAllAsync();

            foreach (var detail in vm.ReceiptDetails)
            {
                var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                if (product != null)
                {
                    detail.ProductName = product.Name;
                    detail.Cost = product.Cost;
                    if (detail.UnitPrice == 0)
                        detail.UnitPrice = product.SellingPrice;
                }
                else
                {
                    detail.ProductName ??= "منتج غير معروف";
                    detail.Cost = detail.Cost != 0 ? detail.Cost : 0;
                }
            }
        }



        // -------------------- REVIEW --------------------
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var receipt = await _receiptService.GetReceiptAsync(id);
            if (receipt == null) return NotFound();

            var vm = new ReceiptListVM
            {
                Id = receipt.Id,
                Date = receipt.Date,
                ReceiptType = receipt.ReceiptType.ToString(),
                ClientName = receipt.Client?.Name,
                PlumberName = receipt.Plumber?.Name,

                Total = receipt.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m))),

                PaidAmount = receipt.PaidAmount,
                RefundAmount = receipt.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * d.RefundQuantity) * (1 - (d.DiscountPercentage / 100m))),

                RemainingAmount = receipt.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m))) - receipt.PaidAmount,

                ReceiptStatus = receipt.Status.ToString(),
                ReceiptDetails = receipt.ReceiptDetails.Select(d => new ReceiptDetailVM
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    RefundQuantity = d.RefundQuantity,
                    DiscountPercentage = d.DiscountPercentage
                }).ToList()
            };

            vm.SupplierSummaries = receipt.ReceiptDetails
            .GroupBy(d =>
            {
                var supplierName = d.Product?.Supplier?.Name;
                if (string.IsNullOrEmpty(supplierName))
                {
                    var lastInReceipt = d.Product?.ReceiptDetails?
                        .Select(rd => rd.Receipt)
                        .Where(r => r.ReceiptType == ReceiptType.In)
                        .OrderByDescending(r => r.Date)
                        .FirstOrDefault();
                    supplierName = lastInReceipt?.Supplier?.Name ?? "غير محدد";
                }
                return supplierName;
            })
            .Select(g => new SupplierSummaryVM
            {
                SupplierName = g.Key,
                DiscountPercentage = g.Any() ? g.Average(d => d.DiscountPercentage) : 0,

                // ✅ before discount
                TotalBeforeDiscount = g.Sum(d => d.UnitPrice * d.Quantity),
                RefundBeforeDiscount = g.Sum(d => d.UnitPrice * d.RefundQuantity),

                // ✅ after discount
                TotalAmount = g.Sum(d => (d.UnitPrice * d.Quantity) * (1 - (d.DiscountPercentage / 100m))),
                RefundAmount = g.Sum(d => (d.UnitPrice * d.RefundQuantity) * (1 - (d.DiscountPercentage / 100m)))
            })
            .ToList();



            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> FixOldReceipts()
        {
            var products = await _productRepo.GetAllAsync();
            var dummyProduct = products.FirstOrDefault(p => p.Name == "Imported/Old Receipt");

            if (dummyProduct == null)
            {
                dummyProduct = new Product { Name = "Imported/Old Receipt" };
                await _productRepo.AddAsync(dummyProduct);
                await _productRepo.SaveAsync();
            }

            var oldReceipts = (await _receiptService.GetAllReceiptsAsync())
                .Where(r => r.IsOld && !r.ReceiptDetails.Any())
                .ToList();

            foreach (var receipt in oldReceipts)
            {
                receipt.ReceiptDetails.Add(new ReceiptDetail
                {
                    ProductId = dummyProduct.Id,
                    Quantity = 1,
                    UnitPrice = receipt.TotalAmount,
                    RefundQuantity = 0,
                    DiscountPercentage = 0
                });

                await _receiptService.UpdateReceiptAsync(receipt); // save changes for each receipt
            }

            return Content($"Fixed {oldReceipts.Count} old receipts.");
        }



        // -------------------- Print --------------------
        public async Task<IActionResult> Print(int id)
        {
            var entity = await _receiptService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            var clients = await _clientRepo.GetAllAsync();

            var vm = new OutReceiptVM
            {
                Id = entity.Id,
                Date = entity.Date,
                PaidAmount = entity.PaidAmount,
                RefundAmount = entity.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * d.RefundQuantity) * (1 - (d.DiscountPercentage / 100m))),
                ClientId = entity.ClientId,
                ReceiptDetails = entity.ReceiptDetails.Select(d => new ReceiptDetailVM
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    RefundQuantity = d.RefundQuantity 
                }).ToList(),
                Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
            };

            return View(vm);
        }





        // -------------------- APPROVE --------------------
        [HttpPost]
        
        public async Task<IActionResult> Approve(int id)
        {
            await _receiptService.ApproveOutReceiptAsync(id);

            TempData["Success"] = "Receipt approved successfully!";
            return RedirectToAction("OutReceipts");
        }

       


        // -------------------- DELETE --------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _receiptService.CancelReceiptAsync(id);

            TempData["Success"] = "Receipt deleted successfully!";
            return RedirectToAction("OutReceipts");
        }

        // -------------------- Edit --------------------


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var receipt = await _receiptService.GetReceiptForEditAsync(id);
            if (receipt == null) return NotFound();

            var products = await _productRepo.GetAllAsync();

            var vm = new EditReceiptVM
            {
                Id = receipt.Id,
                Date = receipt.Date,
                ReceiptType = receipt.ReceiptType,
                SupplierId = receipt.SupplierId,
                ClientId = receipt.ClientId,
                PaidAmount = receipt.PaidAmount,
                RefundAmount = receipt.RefundAmount,
                ReceiptDetails = receipt.ReceiptDetails.Select(d => new ReceiptDetailVM
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "اسم غير معروف", // 🔥 ADDED
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    RefundQuantity = d.RefundQuantity,
                    DiscountPercentage = d.DiscountPercentage,
                    Cost = d.Product?.Cost ?? 0,
                }).ToList(),
                Suppliers = (await _supplierRepo.GetAllAsync()).Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }),
                Clients = (await _clientRepo.GetAllAsync()).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }),
                Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            };

            // 🔥 UPDATED: Use proper pricing based on receipt type
            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new {
                id = p.Id,
                name = p.Name,
                price = vm.ReceiptType == ReceiptType.In ? p.Cost : p.SellingPrice, // ✅ Correct pricing
                cost = p.Cost,
                stock = p.Quantity
            }));

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditReceiptVM vm)
        {
            if (!ModelState.IsValid)
            {
                // 🔥 ENHANCE: Add product information to receipt details
                await EnhanceReceiptDetailsWithProductInfo(vm);

                // repopulate dropdowns
                vm.Suppliers = (await _supplierRepo.GetAllAsync()).Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                vm.Clients = (await _clientRepo.GetAllAsync()).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                vm.Products = (await _productRepo.GetAllAsync()).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });

                // 🔥 ENSURE: Products JSON is available
                ViewBag.ProductsJson = JsonSerializer.Serialize((await _productRepo.GetAllAsync()).Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    price = vm.ReceiptType == ReceiptType.In ? p.Cost : p.SellingPrice,
                    cost = p.Cost,
                    stock = p.Quantity
                }));

                return View(vm);
            }

            // Load original receipt from DB
            var originalReceipt = await _receiptService.GetReceiptForEditAsync(vm.Id);

            if (originalReceipt == null)
                return NotFound();

            // Keep original receipt status
            var receipt = new Receipt
            {
                Id = vm.Id,
                Date = vm.Date,
                ReceiptType = vm.ReceiptType,
                SupplierId = vm.SupplierId,
                ClientId = vm.ClientId,
                PaidAmount = vm.PaidAmount,
                RefundAmount = vm.RefundAmount,
                Status = originalReceipt.Status,   // 🔥 KEY LINE
                ReceiptDetails = vm.ReceiptDetails.Select(d => new ReceiptDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    RefundQuantity = (decimal)d.RefundQuantity,
                    DiscountPercentage = d.DiscountPercentage
                }).ToList()
            };


            try
            {
                if (receipt.ReceiptType == ReceiptType.In)
                {
                    await _receiptService.UpdateReceiptAsync(receipt);
                    return RedirectToAction("InReceipts");
                }
                else
                {
                    await _receiptService.UpdateReceiptAsync(receipt);
                    return RedirectToAction("OutReceipts");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                // 🔥 ENHANCE: Add product information to receipt details on error
                await EnhanceReceiptDetailsWithProductInfo(vm);

                // Repopulate dropdowns
                vm.Suppliers = (await _supplierRepo.GetAllAsync()).Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                vm.Clients = (await _clientRepo.GetAllAsync()).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                vm.Products = (await _productRepo.GetAllAsync()).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });

                ViewBag.ProductsJson = JsonSerializer.Serialize((await _productRepo.GetAllAsync()).Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    price = vm.ReceiptType == ReceiptType.In ? p.Cost : p.SellingPrice,
                    cost = p.Cost,
                    stock = p.Quantity
                }));

                return View(vm);
            }
        }

        // 🔥 ADD THIS METHOD TO ENHANCE PRODUCT INFORMATION
        private async Task EnhanceReceiptDetailsWithProductInfo(EditReceiptVM vm)
        {
            if (vm.ReceiptDetails == null || !vm.ReceiptDetails.Any())
                return;

            // Get all products to match with receipt details
            var products = await _productRepo.GetAllAsync();

            // Enhance receipt details with product information
            foreach (var detail in vm.ReceiptDetails)
            {
                var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                if (product != null)
                {
                    detail.ProductName = product.Name;
                    detail.Cost = product.Cost;
                    // If UnitPrice is not set, use appropriate price based on receipt type
                    if (detail.UnitPrice == 0)
                    {
                        detail.UnitPrice = vm.ReceiptType == ReceiptType.In ? product.Cost : product.SellingPrice;
                    }
                }
                else
                {
                    // Fallback if product not found
                    detail.ProductName = detail.ProductName ?? "منتج غير معروف";
                    detail.Cost = detail.Cost != 0 ? detail.Cost : 0;
                }
            }
        }
        //--------------------Old Receipts -------------

        [HttpGet]
        public IActionResult ImportOld()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportOldReceipts(IFormFile file)
        {
            try
            {
                await _receiptService.ImportOldReceiptsAsync(file);
                TempData["Success"] = "Old receipts imported successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing receipts: {ex.Message}";
            }

            return RedirectToAction("OutReceipts");
        }


        //-------------------- Box --------------------
        [HttpGet]
        public async Task<IActionResult> Box()
        {
            var transactions = await _receiptService.GetBoxTransactionsAsync();

            var adjusted = transactions
           .OrderByDescending(t => t.Date)
           .ToList();

            ViewBag.TotalIn = adjusted.Where(t => t.ReceiptType == "Out").Sum(t => t.Amount);
            ViewBag.TotalOut = adjusted.Where(t => t.ReceiptType == "In").Sum(t => t.Amount);
            ViewBag.Balance = ViewBag.TotalIn - ViewBag.TotalOut;

            return View(adjusted);
        }



        [HttpPost]
        public async Task<IActionResult> AddExternalPayment(decimal amount, string description)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                await _receiptService.AddExternalPaymentAsync(amount,false, description, userName);

                return Json(new { success = true, message = "تمت إضافة المصروف الخارجي بنجاح." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }


        // -------------------- HELPERS --------------------
        private async Task<ReceiptVM> BuildInReceiptVM()
        {
            var suppliers = await _supplierRepo.GetAllAsync();
            var products = await _productRepo.GetAllAsync();

            var vm = new ReceiptVM
            {
                ReceiptType = ReceiptType.In,
                Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }),
                Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }),
                ReceiptDetails = new List<ReceiptDetailVM>()
            };

            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Cost,
                stock = p.Quantity
            }));

            return vm;
        }


        private async Task PopulateInLookups(ReceiptVM vm)
        {
            var suppliers = await _supplierRepo.GetAllAsync();
            var products = await _productRepo.GetAllAsync();

            vm.Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            vm.Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });

            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Cost,
                stock = p.Quantity
            }));
        }

        private async Task<OutReceiptVM> BuildOutReceiptVM()
        {
            var clients = await _clientRepo.GetAllAsync();
            var products = await _productRepo.GetAllAsync();
            var plumber = await _plumberRepo.GetAllAsync();

            var vm = new OutReceiptVM
            {
                Clients = clients.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }),
                Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }),
                Plumbers = plumber.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }),
                ReceiptDetails = new List<ReceiptDetailVM>()
            };

            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.SellingPrice,
                cost = p.Cost,
                stock = p.Quantity
            }));

            return vm;
        }
        private async Task PopulateOutLookups(OutReceiptVM vm)
        {
            var clients = await _clientRepo.GetAllAsync();
            var products = await _productRepo.GetAllAsync();
            var plumber = await _plumberRepo.GetAllAsync();

            vm.Clients = clients.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = vm.ClientId == c.Id
            });
            vm.Plumbers = plumber.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = vm.PlumberId == c.Id
            });

            vm.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = vm.ReceiptDetails != null && vm.ReceiptDetails.Any(d => d.ProductId == p.Id)
            });

            // 🔑 Use SellingPrice like BuildOutReceiptVM
            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.SellingPrice, // ✅ match BuildOutReceiptVM
                cost = p.Cost,
                stock = p.Quantity
            }));

            // Ensure ReceiptDetails is not null
            vm.ReceiptDetails ??= new List<ReceiptDetailVM>();
        }

    }
}
