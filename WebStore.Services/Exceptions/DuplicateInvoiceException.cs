namespace WebStore.Services.Exceptions;

/// <summary>
/// Zamówienie ma już fakturę (relacja 1:1). Rzucany także wtedy, gdy duplikat powstał równolegle
/// i został wykryty dopiero przez unikalny indeks bazy.
/// </summary>
public class DuplicateInvoiceException : InvalidOperationException
{
    public DuplicateInvoiceException(int orderId, Exception? innerException = null)
        : base($"Zamówienie {orderId} ma już fakturę.", innerException)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }
}
