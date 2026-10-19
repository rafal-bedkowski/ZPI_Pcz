using System.ComponentModel.DataAnnotations;

namespace WebStore.ViewModels.VM;

public class AddOrUpdateInvoiceVm
{
    public int? Id { get; set; }

    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrderId { get; set; }
}
