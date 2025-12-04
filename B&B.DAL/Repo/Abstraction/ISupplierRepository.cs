using B_B.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Repo.Abstraction
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier> GetSupplierWithReceiptsAsync(int id);
        Task AddPaymentToSupplierAsync(int supplierId, decimal amount, string createdBy = "النظام");
        Task AddPaymentAsync(int receiptId, decimal amount, string createdBy = "النظام");

    }

}
