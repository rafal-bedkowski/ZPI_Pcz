namespace WebStore.Model.DataModels;

public class OrderProduct
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    // Cena produktu zamrożona w chwili złożenia zamówienia.
    public decimal UnitPrice { get; set; }
}
