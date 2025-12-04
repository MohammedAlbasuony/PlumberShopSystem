using B_B.DAL.Entity;

public class Payment
{
    public int Id { get; set; }

    // 👇 This stays required for normal receipts,
    // but we’ll make it nullable for external payments
    public int? ReceiptId { get; set; }
    public Receipt? Receipt { get; set; }

    public decimal Amount { get; set; } // + for incoming, - for outgoing
    public DateTime Date { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; }

    // ✅ New fields for external payments
    public bool IsExternal { get; set; } = false;     // true => not linked to a receipt
    public string? Description { get; set; }          // "Electricity bill", "Rent", etc.
}
