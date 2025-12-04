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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDBcontext _context;

        public ProductRepository(ApplicationDBcontext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _context.Products
                .Where(p => p.Quantity <= threshold)
                .ToListAsync();
        }


    }

}
