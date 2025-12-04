using B_B.DAL.DB;
using B_B.DAL.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Implementation
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDBcontext context)
        {
            // Ensure DB is created / migrations applied
            await context.Database.MigrateAsync();

            // 1️⃣ Check if the supplier already exists
            var supplier = await context.Suppliers
                .FirstOrDefaultAsync(s => s.Name == "جولدن هاوس");

            if (supplier == null)
            {
                supplier = new Supplier
                {
                    Name = "جولدن هاوس"
                };

                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();
            }

            // 2️⃣ Check if a seed receipt already exists for that supplier
            var existingReceipt = await context.Receipts
                .FirstOrDefaultAsync(r => r.SupplierId == supplier.Id && r.CreatedBy == "seed");

            // ✅ Only add if not found
            if (existingReceipt == null)
            {
                var receipt = new Receipt
                {
                    SupplierId = supplier.Id,
                    Date = DateTime.Now,
                    TotalAmount = 60256.34m, // 💰 total owed
                    CreatedBy = "seed"
                };

                context.Receipts.Add(receipt);
                await context.SaveChangesAsync();

                // 3️⃣ Add zero payment (no amount paid yet)
                var payment = new Payment
                {
                    ReceiptId = receipt.Id,
                    Amount = 0m,
                    Date = DateTime.Now,
                    CreatedBy = "Admin"
                };

                context.Payments.Add(payment);
                await context.SaveChangesAsync();
            }
        }
    }
}
