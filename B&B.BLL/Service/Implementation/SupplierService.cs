using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Implementation
{
    public class SupplierService
    {
        private readonly ISupplierRepository _supplierRepo;

        public SupplierService(ISupplierRepository supplierRepo)
        {
            _supplierRepo = supplierRepo;
        }

        public async Task<Supplier> GetSupplierDetailsAsync(int id)
        {
            return await _supplierRepo.GetSupplierWithReceiptsAsync(id);
        }
    }

}
