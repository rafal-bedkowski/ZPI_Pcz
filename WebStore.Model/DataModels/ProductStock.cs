namespace WebStore.Model.DataModels;

public class ProductStock
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int StationaryStoreId { get; set; }
    public StationaryStore? StationaryStore { get; set; }

    public int Quantity { get; set; }
}
