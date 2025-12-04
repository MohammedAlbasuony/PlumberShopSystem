using B_B.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Abstraction
{
    public interface IStockService
    {
        Task<IEnumerable<Product>> GetCurrentStockAsync();
        Task<Product> GetProductStockAsync(int productId);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);

        Task<IEnumerable<Receipt>> GetPurchasesBySupplierAsync(int supplierId);
        Task<IEnumerable<Receipt>> GetSalesByClientAsync(int clientId);

        Task ImportProductsFromExcelAsync(string filePath);
        Task UpdateProductsFromExcelAsync(string filePath);
    }

}
