using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.Entity
{
    public class Plumber
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; } = string.Empty;


        // Navigation - A plumber can work on many OutReceipts
        public ICollection<Receipt> OutReceipts { get; set; } = new List<Receipt>();
    }
}
