namespace WebStore.ViewModels.VM;

public class OrderProductVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    // Cena zamrożona w chwili złożenia zamówienia.
    public decimal UnitPrice { get; set; }
}
