using System.ComponentModel.DataAnnotations;

namespace WebStore.ViewModels.VM;

public class AddOrUpdateAddressVm
{
    public int? Id { get; set; }

    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string BuildingNumber { get; set; } = string.Empty;

    public string? ApartmentNumber { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;
}
