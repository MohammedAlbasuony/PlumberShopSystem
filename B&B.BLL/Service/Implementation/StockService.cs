using B_B.BLL.Service.Abstraction;
using B_B.DAL.DB;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Implementation
{
    public class StockService : IStockService
    {
        private readonly IProductRepository _productRepo;
        private readonly IReceiptRepository _receiptRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IClientRepository _clientRepo;
        private readonly ApplicationDBcontext _context;

        public StockService(
            IProductRepository productRepo,
            IReceiptRepository receiptRepo,
            ISupplierRepository supplierRepo,
            IClientRepository clientRepo,
            ApplicationDBcontext dBcontext)
        {
            _productRepo = productRepo;
            _receiptRepo = receiptRepo;
            _supplierRepo = supplierRepo;
            _clientRepo = clientRepo;
            _context = dBcontext;
        }

        // 🔹 Current stock list
       
        public async Task<IEnumerable<Product>> GetCurrentStockAsync()
        {
            return await _context.Products
                .Include(p => p.Supplier) // product's direct supplier
                .Include(p => p.ReceiptDetails)
                    .ThenInclude(rd => rd.Receipt)
                        .ThenInclude(r => r.Supplier) // supplier from receipts
                .ToListAsync();
        }


        // 🔹 Single product stock
       public async Task<Product?> GetProductStockAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }


        // 🔹 Low stock products
        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _productRepo.GetLowStockAsync(threshold);
        }

        // 🔹 Purchases from a specific supplier (In Receipts)
        public async Task<IEnumerable<Receipt>> GetPurchasesBySupplierAsync(int supplierId)
        {
            var supplier = await _supplierRepo.GetSupplierWithReceiptsAsync(supplierId);
            return supplier?.Receipts.Where(r => r.ReceiptType == ReceiptType.In);
        }

        // 🔹 Sales to a specific client (Out Receipts)
        public async Task<IEnumerable<Receipt>> GetSalesByClientAsync(int clientId)
        {
            var client = await _clientRepo.GetClientWithReceiptsAsync(clientId);
            return client?.Receipts.Where(r => r.ReceiptType == ReceiptType.Out);
        }

        // 🔹 Stock summary (dictionary: ProductName -> Quantity)


        public async Task ImportProductsFromExcelAsync(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0]; // first sheet
            int rowCount = worksheet.Dimension.Rows;

            // Cache suppliers/products to avoid re-querying for every row
            var existingProducts = (await _productRepo.GetAllAsync()).ToList();
            var existingSuppliers = (await _supplierRepo.GetAllAsync()).ToList();

            for (int row = 2; row <= rowCount; row++) // skip header row
            {
                var name = worksheet.Cells[row, 1].Text?.Trim();
                var sellingPriceText = worksheet.Cells[row, 2].Text;
                var costText = worksheet.Cells[row, 3].Text;
                var initialStockText = worksheet.Cells[row, 4].Text;
                var supplierName = worksheet.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                decimal sellingPrice = decimal.TryParse(sellingPriceText, out var sp) ? sp : 0;
                decimal cost = decimal.TryParse(costText, out var c) ? c : 0;
                int initialStock = int.TryParse(initialStockText, out var stock) ? stock : 0;

                // 🔹 Handle supplier
                Supplier? supplier = null;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    supplier = existingSuppliers.FirstOrDefault(s => s.Name == supplierName);
                    if (supplier == null)
                    {
                        supplier = new Supplier { Name = supplierName };
                        await _supplierRepo.AddAsync(supplier);
                        await _supplierRepo.SaveAsync(); // ✅ get real Id
                        existingSuppliers.Add(supplier);
                    }
                }

                // 🔹 Handle product
                var product = existingProducts.FirstOrDefault(p => p.Name == name);
                if (product == null)
                {
                    // ✅ Add new product
                    product = new Product
                    {
                        Name = name,
                        SellingPrice = sellingPrice,
                        Cost = cost,
                        InitialStock = initialStock,
                        Purchases = 0,
                        Sold = 0,
                        Refunds = 0,
                        SupplierId = supplier?.Id
                    };

                    await _productRepo.AddAsync(product);
                    await _productRepo.SaveAsync(); // ✅ ensures Product.Id is real (no temporary value)
                    existingProducts.Add(product);
                }
                else
                {
                    // ✅ Update existing product
                    product.SellingPrice = sellingPrice;
                    product.Cost = cost;

                    if (initialStock > 0)
                        product.InitialStock = initialStock;

                    if (supplier != null)
                        product.SupplierId = supplier.Id;

                    _productRepo.Update(product);
                }
            }

            // ✅ Save all updates at the end
            await _productRepo.SaveAsync();
        }
        public async Task UpdateProductsFromExcelAsync(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null) return;

            int rowCount = worksheet.Dimension.Rows;

            // Get all existing products
            var existingProducts = (await _productRepo.GetAllAsync()).ToList();

            for (int row = 2; row <= rowCount; row++) // skip header
            {
                var name = worksheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                // Parse numeric values
                decimal sellingPrice = ParseDecimal(worksheet.Cells[row, 2].Text);
                decimal cost = ParseDecimal(worksheet.Cells[row, 3].Text);
                Decimal initialStock = ParseDecimal(worksheet.Cells[row, 4].Text);

                // Find existing product
                var product = existingProducts.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (product != null)
                {
                    product.SellingPrice = sellingPrice;
                    product.Cost = cost;

                    if (initialStock > 0)
                        product.InitialStock = initialStock;

                    _productRepo.Update(product);
                }
            }

            await _productRepo.SaveAsync();
        }

        // -------------------- Helpers --------------------
        private decimal ParseDecimal(string text)
        {
            return decimal.TryParse(text?.Trim(), System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var value)
                                    ? value : 0;
        }

        private int ParseInt(string text)
        {
            return int.TryParse(text?.Trim(), out var value) ? value : 0;
        }



    }

}
