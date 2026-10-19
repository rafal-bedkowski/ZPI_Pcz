namespace WebStore.Model.DataModels;

public class Customer : User
{
    public IList<Order> Orders { get; set; } = new List<Order>();

    public int? BillingAddressId { get; set; }
    public Address? BillingAddress { get; set; }

    public int? ShippingAddressId { get; set; }
    public Address? ShippingAddress { get; set; }
}
