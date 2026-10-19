namespace WebStore.Model.DataModels;

public class StationaryStore
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int AddressId { get; set; }
    public Address? Address { get; set; }

    public IList<StationaryStoreEmployee> StationaryStoreEmployee { get; set; } = new List<StationaryStoreEmployee>();
    public IList<ProductStock> ProductStock { get; set; } = new List<ProductStock>();
}
