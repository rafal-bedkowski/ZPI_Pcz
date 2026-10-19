namespace WebStore.ViewModels.VM;

public class ProductVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[]? ImageBytes { get; set; }
    public decimal Price { get; set; }
    public float Weight { get; set; }

    // Suma ilości produktu we wszystkich sklepach stacjonarnych.
    public int Quantity { get; set; }
}
