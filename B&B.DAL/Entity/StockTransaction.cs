namespace B_B.DAL.Entity
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int QuantityChanged { get; set; }
        public DateTime TransactionDate { get; set; }

        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }
    }
}
