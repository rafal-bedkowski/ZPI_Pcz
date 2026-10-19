namespace WebStore.ViewModels.VM;

public class StoreVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AddressVm? Address { get; set; }
}
