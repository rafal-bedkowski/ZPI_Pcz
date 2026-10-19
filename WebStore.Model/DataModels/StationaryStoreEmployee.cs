namespace WebStore.Model.DataModels;

public class StationaryStoreEmployee : User
{
    public int? StationaryStoreId { get; set; }
    public StationaryStore? StationaryStore { get; set; }
}
