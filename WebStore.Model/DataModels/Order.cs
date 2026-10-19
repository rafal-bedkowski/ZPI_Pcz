namespace WebStore.Model.DataModels;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public long TrackingNumber { get; set; }

    public IList<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    public Invoice? Invoice { get; set; }

    // Wyliczane z zamrożonych cen pozycji, więc późniejsza zmiana ceny produktu nie zmienia sumy.
    public decimal TotalAmount => OrderProducts.Sum(op => op.UnitPrice * op.Quantity);
}
