using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Entity
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Phone { get; set; }

        public ICollection<Product>? Products { get; set; }
        public ICollection<Receipt>? Receipts { get; set; }
    }
}
