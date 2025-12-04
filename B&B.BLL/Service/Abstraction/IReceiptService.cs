using B_B.BLL.ViewModels;
using B_B.DAL.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Abstraction
{
    public interface IReceiptService
    {
        Task<Receipt> GetReceiptAsync(int id);
        Task<IEnumerable<Receipt>> GetAllReceiptsAsync();
        Task AddInReceiptAsync(Receipt receipt, Supplier? newSupplier = null);
        Task<bool> ApproveOutReceiptAsync(int id);
        Task<bool> CancelReceiptAsync(int id);
        Task<int> CreateOutReceiptDraftAsync(Receipt receipt, Client? newClient = null, Plumber? newPlumber = null) ;
        Task ImportOldReceiptsAsync(IFormFile file);

        Task<Receipt?> GetByIdAsync(int id);
        Task<List<BoxTransactionVM>> GetBoxTransactionsAsync();
        Task AddExternalPaymentAsync(decimal amount, bool isInflow, string description, string createdBy);

        // In IReceiptService
        Task<Receipt> GetReceiptForEditAsync(int id);
        Task UpdateReceiptAsync(Receipt updatedReceipt);

    }

}
