namespace WebStore.Model.DataModels;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;

    public IList<Product> Products { get; set; } = new List<Product>();
}
