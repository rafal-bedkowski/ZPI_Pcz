using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class InvoiceServiceTests
{
    private readonly IInvoiceService _service;
    private readonly ApplicationDbContext _db;
    private readonly TestLogger _logger;

    public InvoiceServiceTests(IInvoiceService service, ApplicationDbContext db, TestLogger logger)
    {
        _service = service;
        _db = db;
        _logger = logger;
    }

    private int NewOrder()
    {
        var order = new Order { CustomerId = TestData.CustomerId, OrderDate = DateTime.Today, DeliveryDate = DateTime.Today };
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MonitorId, Quantity = 1, UnitPrice = 1000m });
        _db.Orders.Add(order);
        _db.SaveChanges();
        return order.Id;
    }

    private static AddOrUpdateInvoiceVm NewInvoice(int orderId, int? id = null, string number = "FV/1") => new()
    {
        Id = id, InvoiceNumber = number, IssueDate = new DateTime(2025, 1, 10),
        PaymentDueDate = new DateTime(2025, 1, 24), TotalAmount = 1000m, OrderId = orderId
    };

    [Fact]
    public void US_01_12_Structure_FollowsConventions()
    {
        Assert.True(typeof(IInvoiceService).IsPublic);
        Assert.True(typeof(InvoiceService).IsSubclassOf(typeof(BaseService)));
        Assert.Contains(typeof(IInvoiceService), typeof(InvoiceService).GetInterfaces());
        Assert.EndsWith("Vm", typeof(InvoiceVm).Name);
    }

    [Fact]
    public void US_01_12_AddOrUpdateInvoice_ForOrderWithoutInvoice_IsPersistedAndLinked()
    {
        var orderId = NewOrder();

        var result = _service.AddOrUpdateInvoice(NewInvoice(orderId));

        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(result.Id, _db.Orders.Include(o => o.Invoice).Single(o => o.Id == orderId).Invoice!.Id);
    }

    [Fact]
    public void US_01_12_AddOrUpdateInvoice_SecondInvoiceForOrder_FailsAndSavesNothing()
    {
        var orderId = NewOrder();
        _service.AddOrUpdateInvoice(NewInvoice(orderId));

        Assert.Throws<InvalidOperationException>(() => _service.AddOrUpdateInvoice(NewInvoice(orderId, number: "FV/2")));

        Assert.Equal(1, _db.Invoices.Count());
    }

    [Fact]
    public void US_01_12_AddOrUpdateInvoice_ExistingInvoiceForSameOrder_IsUpdated()
    {
        var orderId = NewOrder();
        var created = _service.AddOrUpdateInvoice(NewInvoice(orderId));

        var updated = _service.AddOrUpdateInvoice(NewInvoice(orderId, created.Id, "FV/zmieniona"));

        Assert.Equal("FV/zmieniona", updated.InvoiceNumber);
        Assert.Equal(1, _db.Invoices.Count());
    }

    [Fact]
    public void US_01_12_AddOrUpdateInvoice_UnknownOrder_Fails()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateInvoice(NewInvoice(999)));

        Assert.Empty(_db.Invoices);
    }

    [Fact]
    public void US_01_12_AddOrUpdateInvoice_UnknownId_ThrowsNotFound()
    {
        var orderId = NewOrder();

        Assert.Throws<KeyNotFoundException>(() => _service.AddOrUpdateInvoice(NewInvoice(orderId, 999)));
        Assert.True(_logger.HasError<KeyNotFoundException>());
    }

    [Fact]
    public void US_01_12_GetInvoice_AndGetInvoices_FilterCorrectly()
    {
        var created = _service.AddOrUpdateInvoice(NewInvoice(NewOrder()));

        Assert.Equal("FV/1", _service.GetInvoice(i => i.Id == created.Id)!.InvoiceNumber);
        Assert.Null(_service.GetInvoice(i => i.InvoiceNumber == "brak"));
        Assert.Single(_service.GetInvoices(i => i.InvoiceNumber == "FV/1"));
        Assert.Equal(_db.Invoices.Count(), _service.GetInvoices().Count());
    }

    [Fact]
    public void US_01_12_NullArguments_ThrowArgumentNullAndAreLogged()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddOrUpdateInvoice(null!));
        Assert.Throws<ArgumentNullException>(() => _service.GetInvoice(null!));

        Assert.Equal(2, _logger.Entries.Count(e => e.Exception is ArgumentNullException));
    }
}
