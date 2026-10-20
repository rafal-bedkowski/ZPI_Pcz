using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class InvoiceService : BaseService, IInvoiceService
{
    public InvoiceService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }

    public InvoiceVm AddOrUpdateInvoice(AddOrUpdateInvoiceVm model) => Execute(nameof(AddOrUpdateInvoice), () =>
    {
        ArgumentNullException.ThrowIfNull(model);

        Invoice? existing = null;
        if (model.Id is not null and not 0)
        {
            existing = DbContext.Invoices.FirstOrDefault(i => i.Id == model.Id)
                       ?? throw NotFound(nameof(Invoice), model.Id.Value);
        }

        var order = DbContext.Orders.Include(o => o.OrderProducts).FirstOrDefault(o => o.Id == model.OrderId)
                    ?? throw NotFound(nameof(Order), model.OrderId);

        // Zamówienie ma co najwyżej jedną fakturę (relacja 1:1).
        if (DbContext.Invoices.Any(i => i.OrderId == model.OrderId && i.Id != (existing != null ? existing.Id : 0)))
        {
            throw new InvalidOperationException($"Zamówienie {model.OrderId} ma już fakturę.");
        }

        Invoice invoice;
        if (existing is null)
        {
            invoice = Mapper.Map<Invoice>(model);
            DbContext.Invoices.Add(invoice);
        }
        else
        {
            invoice = existing;
            Mapper.Map(model, invoice);
        }

        // Kwota faktury zawsze pochodzi z zamówienia, wywołujący nie może jej podać.
        invoice.TotalAmount = order.TotalAmount;

        DbContext.SaveChanges();
        return Mapper.Map<InvoiceVm>(invoice);
    });

    public InvoiceVm? GetInvoice(Expression<Func<Invoice, bool>> filter) => Execute(nameof(GetInvoice), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<InvoiceVm?>(DbContext.Invoices.FirstOrDefault(filter));
    });

    public IEnumerable<InvoiceVm> GetInvoices(Expression<Func<Invoice, bool>>? filter = null) =>
        Execute(nameof(GetInvoices), () =>
        {
            IQueryable<Invoice> query = DbContext.Invoices;
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<InvoiceVm>>(query.ToList());
        });
}
