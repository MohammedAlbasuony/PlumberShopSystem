using B_B.DAL.DB;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B_B.DAL.Repo.Implementation
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly ApplicationDBcontext _context;

        public SupplierRepository(ApplicationDBcontext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllWithReceiptsAsync()
        {
            return await _context.Suppliers
                .Include(s => s.Receipts)
                .ToListAsync();
        }

        public async Task<Supplier> GetSupplierWithReceiptsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.Receipts)
                    .ThenInclude(r => r.ReceiptDetails)
                        .ThenInclude(rd => rd.Product)
                .Include(s => s.Receipts)
                    .ThenInclude(r => r.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        public async Task AddPaymentAsync(int receiptId, decimal amount, string createdBy = "النظام")
        {
            var receipt = await _context.Receipts.FindAsync(receiptId);
            if (receipt == null) return;

            // Update receipt summary
            receipt.PaidAmount += amount;

            if (receipt.PaidAmount < 0)
                receipt.PaidAmount = 0;

            if (receipt.TotalAmount > 0 && receipt.PaidAmount > receipt.TotalAmount)
                receipt.PaidAmount = receipt.TotalAmount;

            // Save payment (amount can be negative = refund)
            var payment = new Payment
            {
                ReceiptId = receipt.Id,
                Amount = amount,
                CreatedBy = createdBy,
                Date = DateTime.Now
            };

            await _context.Payments.AddAsync(payment);
            _context.Receipts.Update(receipt);

            await _context.SaveChangesAsync();
        }


        public async Task AddPaymentToSupplierAsync(int supplierId, decimal amount, string createdBy = "النظام")
        {
            var receipts = await _context.Receipts
                .Where(r => r.SupplierId == supplierId)
                .OrderBy(r => r.Date) // oldest first (FIFO), change to OrderByDescending if you prefer newest first
                .ToListAsync();

            if (!receipts.Any()) return;

            decimal remainingAmount = amount;

            foreach (var receipt in receipts)
            {
                if (remainingAmount == 0) break;

                // How much is left unpaid on this receipt
                var unpaid = (receipt.TotalAmount - receipt.RefundAmount) - receipt.PaidAmount;

                if (unpaid == 0) continue;

                // Apply payment to this receipt
                var pay = Math.Min(unpaid, remainingAmount);

                receipt.PaidAmount += pay;

                var payment = new Payment
                {
                    ReceiptId = receipt.Id,
                    Amount = pay,
                    CreatedBy = createdBy,
                    Date = DateTime.Now
                };

                await _context.Payments.AddAsync(payment);

                remainingAmount -= pay;
            }

            await _context.SaveChangesAsync();
        }

    }
}
