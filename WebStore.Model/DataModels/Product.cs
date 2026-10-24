namespace WebStore.Model.DataModels;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[]? ImageBytes { get; set; }
    public decimal Price { get; set; }
    public float Weight { get; set; }

    // Ukryty produkt nie jest oferowany ani przyjmowany w zamówieniach, ale zostaje w bazie z powodu historii zamówień.
    public bool IsHidden { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public IList<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
}
