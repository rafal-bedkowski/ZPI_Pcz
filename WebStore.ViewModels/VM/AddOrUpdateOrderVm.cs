using System.ComponentModel.DataAnnotations;

namespace WebStore.ViewModels.VM;

public class AddOrUpdateOrderVm
{
    public int? Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public long TrackingNumber { get; set; }

    [Required]
    public IList<AddOrUpdateOrderProductVm> Products { get; set; } = new List<AddOrUpdateOrderProductVm>();
}
