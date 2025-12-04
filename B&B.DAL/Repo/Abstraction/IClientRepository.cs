using B_B.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Repo.Abstraction
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<Client> GetClientWithReceiptsAsync(int id);
        Task AddPaymentAsync(int receiptId, decimal amount, string createdBy = "النظام");
        Task<IEnumerable<Client>> GetAllWithReceiptsAsync();
    }

}
