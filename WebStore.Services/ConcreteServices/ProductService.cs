using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class ProductService : BaseService, IProductService
{
    public ProductService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }

    public ProductVm AddOrUpdateProduct(AddOrUpdateProductVm model) => Execute(nameof(AddOrUpdateProduct), () =>
    {
        ArgumentNullException.ThrowIfNull(model);

        // Ukryty produkt jest traktowany jak nieistniejący, więc nie można go modyfikować.
        Product? existing = null;
        if (model.Id is not null and not 0)
        {
            existing = DbContext.Products.FirstOrDefault(p => p.Id == model.Id && !p.IsHidden)
                       ?? throw NotFound(nameof(Product), model.Id.Value);
        }

        // Jawna kontrola powiązań daje czytelny błąd zamiast wyjątku klucza obcego z bazy.
        if (!DbContext.Categories.Any(c => c.Id == model.CategoryId))
        {
            throw NotFound(nameof(Category), model.CategoryId);
        }

        if (!DbContext.Suppliers.Any(s => s.Id == model.SupplierId))
        {
            throw NotFound(nameof(Supplier), model.SupplierId);
        }

        Product product;
        if (existing is null)
        {
            product = Mapper.Map<Product>(model);
            DbContext.Products.Add(product);
        }
        else
        {
            product = existing;
            Mapper.Map(model, product);
        }

        DbContext.SaveChanges();
        return GetProductVm(product.Id);
    });

    public ProductVm? GetProduct(Expression<Func<Product, bool>> filter) => Execute(nameof(GetProduct), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<ProductVm?>(DbContext.Products.Include(p => p.ProductStocks).Where(p => !p.IsHidden).FirstOrDefault(filter));
    });

    public IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filter = null) =>
        Execute(nameof(GetProducts), () =>
        {
            IQueryable<Product> query = DbContext.Products.Include(p => p.ProductStocks).Where(p => !p.IsHidden);
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<ProductVm>>(query.ToList());
        });

    public void HideProduct(int id) => Execute(nameof(HideProduct), () =>
    {
        var product = DbContext.Products.FirstOrDefault(p => p.Id == id && !p.IsHidden)
                      ?? throw NotFound(nameof(Product), id);
        product.IsHidden = true;
        DbContext.SaveChanges();
        return true;
    });

    private ProductVm GetProductVm(int id) =>
        Mapper.Map<ProductVm>(DbContext.Products.Include(p => p.ProductStocks).First(p => p.Id == id));
}
