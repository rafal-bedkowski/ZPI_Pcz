namespace WebStore.Model.DataModels;

public class Supplier : User
{
    public IList<Product> Products { get; set; } = new List<Product>();
}
