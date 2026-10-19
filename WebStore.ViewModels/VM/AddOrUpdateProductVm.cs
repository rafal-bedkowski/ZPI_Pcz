using System.ComponentModel.DataAnnotations;

namespace WebStore.ViewModels.VM;

public class AddOrUpdateProductVm
{
    public int? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public byte[]? ImageBytes { get; set; }

    [Range(0, 1000000000)]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public float Weight { get; set; }

    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
}
