namespace B_B.DAL.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Decimal? InitialStock { get; set; }

        public Decimal Quantity => (Purchases + (InitialStock ?? 0) + Refunds - Sold);

        public Decimal Sold { get; set; }
        public Decimal Purchases { get; set; }
        public Decimal Refunds { get; set; }

        public decimal Cost { get; set; }
        public decimal SellingPrice { get; set; }

        // 🔹 Supplier relation
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<ReceiptDetail>? ReceiptDetails { get; set; }
    }
}
