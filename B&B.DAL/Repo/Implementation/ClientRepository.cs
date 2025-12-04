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
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        private readonly ApplicationDBcontext _context;

        public ClientRepository(ApplicationDBcontext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Client>> GetAllWithReceiptsAsync()
        {
            return await _context.Clients
                .Include(c => c.Receipts)
                .ToListAsync();
        }
        public async Task<Client> GetClientWithReceiptsAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Receipts)
                    .ThenInclude(r => r.ReceiptDetails)
                        .ThenInclude(rd => rd.Product)
                .Include(c => c.Receipts)
                    .ThenInclude(r => r.Payments) // ✅ Include payments for accurate PaidAmount
                .AsNoTracking() // ✅ Improves performance for read-only queries
                .FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task AddPaymentAsync(int receiptId, decimal amount, string createdBy = "النظام")
        {
            var receipt = await _context.Receipts.FindAsync(receiptId);
            if (receipt == null) return;

            // Update receipt summary
            receipt.PaidAmount += amount;

           

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




    }

}
