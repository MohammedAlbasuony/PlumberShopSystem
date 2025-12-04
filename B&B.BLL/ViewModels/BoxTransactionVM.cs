namespace B_B.BLL.ViewModels
{
    public class BoxTransactionVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ReceiptType { get; set; } // In / Out
        public string SupplierOrCustomer { get; set; }
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; }
        public bool IsExternal { get; set; }
        public string? Description { get; set; }
    }

}
