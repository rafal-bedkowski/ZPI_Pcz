using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.Tests.Infrastructure;
using WebStore.ViewModels.VM;

namespace WebStore.Tests.UnitTests;

public class InvoiceServiceUnitTests : BaseUnitTests
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceServiceUnitTests(ApplicationDbContext dbContext, IInvoiceService invoiceService) : base(dbContext)
    {
        _invoiceService = invoiceService;
    }

    // Faktura jest powiązana z zamówieniem 1:1, więc każda faktura w teście dostaje własne zamówienie.
    private int SaveOrder()
    {
        var order = new Order { CustomerId = TestData.CustomerId, OrderDate = DateTime.Today, DeliveryDate = DateTime.Today };
        order.OrderProducts.Add(new OrderProduct { ProductId = TestData.MonitorId, Quantity = 1, UnitPrice = 1000m });
        DbContext.Orders.Add(order);
        DbContext.SaveChanges();
        return order.Id;
    }

    private Invoice SaveInvoice(string number)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = number, IssueDate = new DateTime(2025, 1, 10),
            PaymentDueDate = new DateTime(2025, 1, 24), TotalAmount = 1000m, OrderId = SaveOrder()
        };
        DbContext.Invoices.Add(invoice);
        DbContext.SaveChanges();
        return invoice;
    }

    private static AddOrUpdateInvoiceVm NewInvoiceVm(int orderId, int? id, string number) => new()
    {
        Id = id, InvoiceNumber = number, IssueDate = new DateTime(2025, 1, 10),
        PaymentDueDate = new DateTime(2025, 1, 24), OrderId = orderId
    };

    [Fact]
    public void GetInvoiceTest()
    {
        SaveInvoice("FV/1");
        var saved = SaveInvoice("FV/2");

        var invoice = _invoiceService.GetInvoice(i => i.InvoiceNumber == "FV/2");

        Assert.NotNull(invoice);
        Assert.Equal(saved.Id, invoice.Id);
        Assert.Equal("FV/2", invoice.InvoiceNumber);
        Assert.Null(_invoiceService.GetInvoice(i => i.InvoiceNumber == "FV/999"));
    }

    [Fact]
    public void GetMultipleInvoicesTest()
    {
        var first = SaveInvoice("FV/1");
        var second = SaveInvoice("FV/2");
        var outside = SaveInvoice("FV/3");

        var invoices = _invoiceService.GetInvoices(i => i.Id >= first.Id && i.Id <= second.Id).ToList();

        Assert.Equal(2, invoices.Count);
        Assert.DoesNotContain(invoices, i => i.Id == outside.Id);
    }

    [Fact]
    public void GetAllInvoicesTest()
    {
        SaveInvoice("FV/1");
        SaveInvoice("FV/2");

        var invoices = _invoiceService.GetInvoices().ToList();

        Assert.Equal(DbContext.Invoices.Count(), invoices.Count);
    }

    [Fact]
    public void AddNewInvoiceTest()
    {
        var orderId = SaveOrder();
        var before = DbContext.Invoices.Count();

        var invoice = _invoiceService.AddOrUpdateInvoice(NewInvoiceVm(orderId, null, "FV/10"));

        Assert.Equal("FV/10", invoice.InvoiceNumber);
        Assert.Equal(orderId, invoice.OrderId);
        Assert.Equal(before + 1, DbContext.Invoices.Count());

        DbContext.ChangeTracker.Clear();
        var stored = DbContext.Invoices.AsNoTracking().Single(i => i.Id == invoice.Id);
        Assert.Equal("FV/10", stored.InvoiceNumber);
        Assert.Equal(orderId, stored.OrderId);
        Assert.Equal(1000m, stored.TotalAmount);
        Assert.Equal(new DateTime(2025, 1, 10), stored.IssueDate);
        Assert.Equal(new DateTime(2025, 1, 24), stored.PaymentDueDate);
    }

    [Fact]
    public void UpdateInvoiceTest()
    {
        var saved = SaveInvoice("FV/1");
        var before = DbContext.Invoices.Count();

        var invoice = _invoiceService.AddOrUpdateInvoice(NewInvoiceVm(saved.OrderId, saved.Id, "FV/zmieniona"));

        Assert.Equal(saved.Id, invoice.Id);
        Assert.Equal("FV/zmieniona", invoice.InvoiceNumber);
        Assert.Equal(before, DbContext.Invoices.Count());

        DbContext.ChangeTracker.Clear();
        Assert.Equal(before, DbContext.Invoices.AsNoTracking().Count());
        var stored = DbContext.Invoices.AsNoTracking().Single(i => i.Id == saved.Id);
        Assert.Equal("FV/zmieniona", stored.InvoiceNumber);
        Assert.Equal(saved.OrderId, stored.OrderId);
        Assert.Equal(1000m, stored.TotalAmount);
    }
}
