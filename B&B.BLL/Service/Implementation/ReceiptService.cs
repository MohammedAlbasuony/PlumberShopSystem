using B_B.BLL.Service.Abstraction;
using B_B.BLL.ViewModels;
using B_B.DAL.DB;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Implementation
{
    public class ReceiptService : IReceiptService
    {
        private readonly ApplicationDBcontext _context;
        private readonly IReceiptRepository _receiptRepo;
        private readonly IProductRepository _productRepo;
        private readonly IClientRepository _clientRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IGenericRepository<Plumber> _plumberRepo;

        public ReceiptService(
            IReceiptRepository receiptRepo,
            IProductRepository productRepo,
            IClientRepository clientRepo,
            ISupplierRepository supplierRepo,
            ApplicationDBcontext context,
            IGenericRepository<Plumber> plumberRepo)
        {
            _receiptRepo = receiptRepo;
            _productRepo = productRepo;
            _clientRepo = clientRepo;
            _supplierRepo = supplierRepo;
            _context = context;
            _plumberRepo = plumberRepo;
        }

        // 🔹 Helper to add payment to Box
        private async Task AddPaymentToBoxAsync(Receipt receipt, decimal amount, string source)
        {
            if (amount == 0)
                return;

            var payment = new Payment
            {
                ReceiptId = receipt.Id,
                Amount = amount,
                CreatedBy = source,
                Date = DateTime.Now
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        // Get a single receipt with details
        public async Task<Receipt> GetReceiptAsync(int id)
        {
            return await _receiptRepo.GetReceiptWithDetailsAsync(id);
        }

        // Get all receipts
        public async Task<IEnumerable<Receipt>> GetAllReceiptsAsync()
        {
            return await _receiptRepo.GetAllReceiptsAsync();
        }

        // Add an IN receipt (purchase from supplier → stock increases)
        public async Task AddInReceiptAsync(Receipt receipt, Supplier? newSupplier = null)
        {
            int supplierId;

            if (newSupplier != null)
            {
                await _supplierRepo.AddAsync(newSupplier);
                await _supplierRepo.SaveAsync();
                supplierId = newSupplier.Id;
            }
            else if (receipt.SupplierId.HasValue)
            {
                supplierId = receipt.SupplierId.Value;
            }
            else
            {
                throw new InvalidOperationException("A supplier must be selected or provided.");
            }

            receipt.SupplierId = supplierId;

            // ✅ Calculate totals and refund BEFORE save
            receipt.TotalAmount = receipt.ReceiptDetails.Sum(d =>
                (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));

            receipt.RefundAmount = receipt.ReceiptDetails.Sum(d =>
                d.UnitPrice * d.RefundQuantity * (1 - (d.DiscountPercentage / 100m)));

            if (receipt.PaidAmount < 0)
                receipt.PaidAmount = 0;

            await _receiptRepo.AddAsync(receipt);
            await _receiptRepo.SaveAsync();

            // 🔹 Payment (money going out)
            if (receipt.PaidAmount > 0)
                await AddPaymentToBoxAsync(receipt, -Math.Abs(receipt.PaidAmount), "فاتورة شراء");

            // 🔹 Update product cost, supplier, and stock
            foreach (var detail in receipt.ReceiptDetails)
            {
                var product = await _productRepo.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    product.Cost = detail.UnitPrice;
                    product.Purchases += detail.Quantity;
                    product.Purchases -= detail.RefundQuantity; // ✅ handle refund quantity
                    product.SupplierId = supplierId;

                    _productRepo.Update(product);
                }
            }

            await _productRepo.SaveAsync();
        }





        public async Task<int> CreateOutReceiptDraftAsync(
             Receipt receipt,
             Client? newClient = null,
             Plumber? newPlumber = null)
        {
            if (newClient != null)
            {
                await _clientRepo.AddAsync(newClient);
                await _clientRepo.SaveAsync();
                receipt.ClientId = newClient.Id;
            }

            if (newPlumber != null)
            {
                await _plumberRepo.AddAsync(newPlumber);
                await _plumberRepo.SaveAsync();
                receipt.PlumberId = newPlumber.Id;
            }

            // ✅ Calculate totals and refunds
            receipt.TotalAmount = receipt.ReceiptDetails.Sum(d =>
                (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));

            receipt.RefundAmount = receipt.ReceiptDetails.Sum(d =>
                d.UnitPrice * d.RefundQuantity * (1 - (d.DiscountPercentage / 100m)));

            receipt.Status = ReceiptStatus.Draft;

            await _receiptRepo.AddAsync(receipt);
            await _receiptRepo.SaveAsync();

            return receipt.Id;
        }



        public async Task ImportOldReceiptsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid Excel file.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found.");

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string clientName = worksheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(clientName))
                    continue;

                decimal total = ParseDecimalCell(worksheet.Cells[row, 2]);
                decimal paid = ParseDecimalCell(worksheet.Cells[row, 3]);

                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Name == clientName);

                if (client == null)
                {
                    client = new Client { Name = clientName };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                }

                bool exists = await _context.Receipts.AnyAsync(r =>
                    r.ClientId == client.Id && r.TotalAmount == total &&
                    r.PaidAmount == paid && r.IsOld);

                if (exists)
                    continue;

                var receipt = new Receipt
                {
                    Date = DateTime.Now,
                    ClientId = client.Id,
                    TotalAmount = total,
                    PaidAmount = paid,
                    Status = ReceiptStatus.Approved,
                    ReceiptType = ReceiptType.Out,
                    IsOld = true,
                    CreatedBy = "Excel Import"
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();

                if (paid > 0)
                    await AddPaymentToBoxAsync(receipt, Math.Abs(paid), "Excel Import");
            }
        }

        private static decimal ParseDecimalCell(ExcelRange cell)
        {
            try
            {
                if (cell.Value == null) return 0;

                if (decimal.TryParse(cell.Value.ToString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var result))
                    return result;

                var cleaned = new string(cell.Text.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
                return decimal.TryParse(cleaned,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out result) ? result : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> ApproveOutReceiptAsync(int id)
        {
            var receipt = await _receiptRepo.GetByIdAsync(id);
            if (receipt == null || receipt.Status != ReceiptStatus.Draft)
                return false;

            foreach (var detail in receipt.ReceiptDetails)
            {
                var product = await _productRepo.GetByIdAsync(detail.ProductId);
                if (product == null)
                    throw new Exception($"Product not found: {detail.ProductId}");

                product.Sold += detail.Quantity;
                _productRepo.Update(product);
            }

            receipt.Status = ReceiptStatus.Approved;
            _receiptRepo.Update(receipt);
            await _receiptRepo.SaveAsync();

            if (receipt.PaidAmount > 0)
                await AddPaymentToBoxAsync(receipt, Math.Abs(receipt.PaidAmount), "فاتورة بيع");

            return true;
        }

        public async Task<bool> CancelReceiptAsync(int id)
        {
            var receipt = await _receiptRepo.GetByIdAsync(id);
            if (receipt == null || receipt.Status != ReceiptStatus.Draft)
                return false;

            _receiptRepo.Delete(receipt);
            await _receiptRepo.SaveAsync();
            return true;
        }

        public async Task<Receipt?> GetByIdAsync(int id)
        {
            return await _receiptRepo.GetByIdAsync(id);
        }

        public async Task<Receipt> GetReceiptForEditAsync(int id)
        {
            return await _receiptRepo.GetByIdAsync(id);
        }

        public async Task UpdateReceiptAsync(Receipt updatedReceipt)
        {
            // Load existing receipt (tracked)
            var existing = await _context.Receipts
                .Include(r => r.ReceiptDetails)
                .FirstOrDefaultAsync(r => r.Id == updatedReceipt.Id);

            if (existing == null)
                throw new Exception("Receipt not found");

            // ============================================================
            // 🔥 1) HANDLE DRAFT MODE — NO STOCK CHANGE AT ALL
            // ============================================================
            if (existing.Status == ReceiptStatus.Draft && existing.ReceiptType != ReceiptType.In)
            {
                // Update basic fields
                existing.Date = updatedReceipt.Date;
                existing.SupplierId = updatedReceipt.SupplierId;
                existing.ClientId = updatedReceipt.ClientId;
                existing.PaidAmount = updatedReceipt.PaidAmount;
                existing.ReceiptType = updatedReceipt.ReceiptType;

                // Replace details (no stock effect)
                existing.ReceiptDetails.Clear();
                foreach (var d in updatedReceipt.ReceiptDetails)
                {
                    existing.ReceiptDetails.Add(new ReceiptDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        RefundQuantity = d.RefundQuantity,
                        DiscountPercentage = d.DiscountPercentage
                    });
                }

                // Recalculate totals
                existing.TotalAmount = existing.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));

                existing.RefundAmount = existing.ReceiptDetails.Sum(d =>
                    (d.UnitPrice * d.RefundQuantity) * (1 - (d.DiscountPercentage / 100m)));

                // Save receipt first
                await _context.SaveChangesAsync();

                // 🔹 Add payment to box even in Draft
                var Payments = await _context.Payments
                    .Where(p => p.ReceiptId == existing.Id)
                    .ToListAsync();

                _context.Payments.RemoveRange(Payments);
                await _context.SaveChangesAsync();

                if (existing.PaidAmount > 0)
                {
                    if (existing.ReceiptType == ReceiptType.Out)
                        await AddPaymentToBoxAsync(existing, +Math.Abs(existing.PaidAmount), "تعديل فاتورة بيع (مسودة)");
                    else
                        await AddPaymentToBoxAsync(existing, -Math.Abs(existing.PaidAmount), "تعديل فاتورة شراء (مسودة)");
                }

                return; // stock unchanged, payment updated
            }


            // ============================================================
            // 🔥 2) NORMAL (FINAL) RECEIPT — STOCK MUST BE UPDATED
            // ============================================================

            // Reverse old stock
            foreach (var detail in existing.ReceiptDetails)
            {
                var product = await _productRepo.GetByIdAsync(detail.ProductId);

                if (existing.ReceiptType == ReceiptType.In)
                    product.Purchases -= detail.Quantity;
                else if (existing.ReceiptType == ReceiptType.Out)
                    product.Sold -= (detail.Quantity - detail.RefundQuantity);

                _productRepo.Update(product);
            }

            // Apply new stock
            foreach (var detail in updatedReceipt.ReceiptDetails)
            {
                var product = await _productRepo.GetByIdAsync(detail.ProductId);

                if (updatedReceipt.ReceiptType == ReceiptType.In)
                    product.Purchases += detail.Quantity;
                else if (updatedReceipt.ReceiptType == ReceiptType.Out)
                    product.Sold += (detail.Quantity - detail.RefundQuantity);

                _productRepo.Update(product);
            }

            // Update basic fields
            existing.Date = updatedReceipt.Date;
            existing.SupplierId = updatedReceipt.SupplierId;
            existing.ClientId = updatedReceipt.ClientId;
            existing.PaidAmount = updatedReceipt.PaidAmount;
            existing.ReceiptType = updatedReceipt.ReceiptType;

            // Replace details
            existing.ReceiptDetails.Clear();
            foreach (var d in updatedReceipt.ReceiptDetails)
            {
                existing.ReceiptDetails.Add(new ReceiptDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    RefundQuantity = d.RefundQuantity,
                    DiscountPercentage = d.DiscountPercentage
                });
            }

            // Recalculate totals
            existing.TotalAmount = existing.ReceiptDetails.Sum(d =>
                (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));

            existing.RefundAmount = existing.ReceiptDetails.Sum(d =>
                (d.UnitPrice * d.RefundQuantity) * (1 - (d.DiscountPercentage / 100m)));

            // Save stock + receipt
            await _productRepo.SaveAsync();
            await _context.SaveChangesAsync();

            // Handle payments
            var oldPayments = await _context.Payments
                .Where(p => p.ReceiptId == existing.Id)
                .ToListAsync();

            _context.Payments.RemoveRange(oldPayments);
            await _context.SaveChangesAsync();

            if (existing.PaidAmount > 0)
            {
                if (existing.ReceiptType == ReceiptType.Out)
                    await AddPaymentToBoxAsync(existing, +Math.Abs(existing.PaidAmount), "تعديل فاتورة بيع");
                else
                    await AddPaymentToBoxAsync(existing, -Math.Abs(existing.PaidAmount), "تعديل فاتورة شراء");
            }
        }





        public async Task<List<BoxTransactionVM>> GetBoxTransactionsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Receipt.Client)
                .Include(p => p.Receipt.Supplier)
                .AsNoTracking()
                .ToListAsync();

            var list = payments.Select(p =>
            {
                string type;
                string supplierOrCustomer = "غير محدد";

                if (p.Receipt == null || p.IsExternal)
                {
                    type = p.Amount < 0 ? "Out" : "In"; // negative = Out, positive = In
                    supplierOrCustomer = p.Description ?? "مصروف خارجي";
                }

                else
                {
                    // ✅ The 4 possible cases handled clearly
                    if (p.Receipt.ReceiptType == ReceiptType.Out)
                        type = p.Amount >= 0 ? "Out" : "In"; // Sale: + = In, - = Out
                    else
                        type = p.Amount >= 0 ? "Out" : "In"; // Purchase: + = Out, - = In

                    supplierOrCustomer = p.Receipt.Client?.Name ?? p.Receipt.Supplier?.Name ?? "غير محدد";
                }

                return new BoxTransactionVM
                {
                    Id = p.Id,
                    Date = p.Date,
                    ReceiptType = type,
                    SupplierOrCustomer = supplierOrCustomer,
                    Amount = Math.Abs(p.Amount),
                    CreatedBy = p.CreatedBy ?? "النظام",
                    IsExternal = p.IsExternal,
                    Description = p.Description
                };
            })
            .OrderByDescending(t => t.Date)
            .ToList();

            decimal initialBoxAmount = 26776.93m;

            var openingTransaction = new BoxTransactionVM
            {
                Id = 0,
                Date = list.Any() ? list.Max(t => t.Date).AddSeconds(-1) : DateTime.Now.AddSeconds(-1),
                ReceiptType = "Out",
                SupplierOrCustomer = "رصيد افتتاحي",
                Amount = initialBoxAmount,
                CreatedBy = "النظام",
                IsExternal = false,
                Description = "الرصيد الافتتاحي للصندوق"
            };

            list.Add(openingTransaction);
            list = list.OrderByDescending(t => t.Date).ToList();

            return list;
        }



        public async Task AddExternalPaymentAsync(decimal amount, bool isInflow, string description, string createdBy)
        {
            var payment = new Payment
            {
                Amount = isInflow ? -Math.Abs(amount) : +Math.Abs(amount),
                Description = description,
                IsExternal = true,
                CreatedBy = createdBy,
                Date = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }


    }
}
