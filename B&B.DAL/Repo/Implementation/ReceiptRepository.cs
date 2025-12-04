using B_B.DAL.DB;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Repo.Implementation
{
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        private readonly ApplicationDBcontext _context;

        public ReceiptRepository(ApplicationDBcontext context) : base(context)
        {
            _context = context;
        }
        public async Task AddInReceiptAsync(Receipt receipt)
        {
            foreach (var detail in receipt.ReceiptDetails)
            {
                var normalizedName = detail.Product.Name.Trim().ToLower();

                // ✅ Check if product already exists (case-insensitive)
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == normalizedName);

                if (product != null)
                {
                    

                    // Optional: update price
                    product.Cost = detail.UnitPrice;

                    // Link receipt detail to existing product
                    detail.ProductId = product.Id;
                    detail.Product = null;
                }
                else
                {
                    // Add new product
                    var newProduct = new Product
                    {
                        Name = detail.Product.Name.Trim(),
                        Cost = detail.UnitPrice,
                        Purchases = detail.Quantity
                    };

                    await _context.Products.AddAsync(newProduct);

                    detail.Product = newProduct;
                }
            }

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task<Receipt> GetReceiptWithDetailsAsync(int id)
        {
            return await _context.Receipts
                .Include(r => r.Supplier) // supplier at receipt level (if exists)
                .Include(r => r.Client)
                    .Include(r => r.Plumber) // ✅ add this

                .Include(r => r.ReceiptDetails)
                    .ThenInclude(rd => rd.Product)
                        .ThenInclude(p => p.Supplier) // direct supplier of each product
                .Include(r => r.ReceiptDetails)
                    .ThenInclude(rd => rd.Product)
                        .ThenInclude(p => p.ReceiptDetails)
                            .ThenInclude(prd => prd.Receipt)
                                .ThenInclude(rr => rr.Supplier) // ✅ supplier from other receipts
                .FirstOrDefaultAsync(r => r.Id == id);
        }



        public async Task<IEnumerable<Receipt>> GetReceiptsByTypeAsync(ReceiptType type)
        {
            return await _context.Receipts
                .Where(r => r.ReceiptType == type)
                .Include(r => r.Supplier)
                .Include(r => r.Client)
                .ToListAsync();
        }


        public async Task<IEnumerable<Receipt>> GetAllReceiptsAsync()
        {
            var receipts = await _context.Receipts
                .Include(r => r.Supplier)
                .Include(r => r.Client)
                .Include(r => r.Plumber)
                .Include(r => r.ReceiptDetails)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.Supplier) // add this line
                .AsNoTracking() // optional but improves performance
                .ToListAsync();

            foreach (var r in receipts)
            {
                if (r.ReceiptDetails != null && r.ReceiptDetails.Any())
                {
                    r.RefundAmount = r.ReceiptDetails.Sum(d => d.UnitPrice * d.RefundQuantity);
                    r.TotalAmount = r.ReceiptDetails
                        .Sum(d => (d.UnitPrice * (d.Quantity - d.RefundQuantity)) * (1 - (d.DiscountPercentage / 100m)));
                }
            }

            return receipts;
        }









        public async Task<Receipt?> GetByIdAsync(int id)
        {
            return await _context.Receipts
                .Include(r => r.ReceiptDetails)
                .ThenInclude(d => d.Product)   // ✅ Load product info

                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

       

        public async Task UpdateReceipt(Receipt receipt)
        {
            var existing = await _context.Receipts
                .Include(r => r.ReceiptDetails)
                .FirstOrDefaultAsync(r => r.Id == receipt.Id);

            if (existing == null) throw new Exception("Receipt not found");

            // Update main fields
            existing.Date = receipt.Date;
            existing.SupplierId = receipt.SupplierId;
            existing.ClientId = receipt.ClientId;
            existing.PaidAmount = receipt.PaidAmount;
            existing.TotalAmount = receipt.ReceiptDetails.Sum(d => d.UnitPrice * d.Quantity);

            // Remove old details
            _context.ReceiptDetails.RemoveRange(existing.ReceiptDetails);

            // Add updated details
            existing.ReceiptDetails = receipt.ReceiptDetails;

            await _context.SaveChangesAsync();
        }


    }

}
