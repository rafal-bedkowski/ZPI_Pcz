namespace WebStore.Model.DataModels;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public decimal TotalAmount { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}
