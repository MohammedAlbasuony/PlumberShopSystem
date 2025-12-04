using B_B.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Repo.Abstraction
{
    public interface IReceiptRepository : IGenericRepository<Receipt>
    {
        Task<Receipt> GetReceiptWithDetailsAsync(int id);
        Task<IEnumerable<Receipt>> GetReceiptsByTypeAsync(ReceiptType type);
        Task AddInReceiptAsync(Receipt receipt);
        Task<IEnumerable<Receipt>> GetAllReceiptsAsync();
        Task UpdateReceipt(Receipt receipt);
        Task<Receipt?> GetByIdAsync(int id);

    }

}
