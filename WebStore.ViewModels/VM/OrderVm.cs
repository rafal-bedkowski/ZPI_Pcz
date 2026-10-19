namespace WebStore.ViewModels.VM;

public class OrderVm
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public long TrackingNumber { get; set; }
    public int? InvoiceId { get; set; }
    public IList<OrderProductVm> Products { get; set; } = new List<OrderProductVm>();
}
