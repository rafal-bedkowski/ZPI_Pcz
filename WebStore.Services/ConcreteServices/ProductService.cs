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

        Product product;
        if (model.Id is null or 0)
        {
            product = Mapper.Map<Product>(model);
            DbContext.Products.Add(product);
        }
        else
        {
            product = DbContext.Products.FirstOrDefault(p => p.Id == model.Id)
                      ?? throw NotFound(nameof(Product), model.Id.Value);
            Mapper.Map(model, product);
        }

        DbContext.SaveChanges();
        return GetProductVm(product.Id);
    });

    public ProductVm? GetProduct(Expression<Func<Product, bool>> filter) => Execute(nameof(GetProduct), () =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Mapper.Map<ProductVm?>(DbContext.Products.Include(p => p.ProductStocks).FirstOrDefault(filter));
    });

    public IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filter = null) =>
        Execute(nameof(GetProducts), () =>
        {
            IQueryable<Product> query = DbContext.Products.Include(p => p.ProductStocks);
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return Mapper.Map<List<ProductVm>>(query.ToList());
        });

    private ProductVm GetProductVm(int id) =>
        Mapper.Map<ProductVm>(DbContext.Products.Include(p => p.ProductStocks).First(p => p.Id == id));
}
