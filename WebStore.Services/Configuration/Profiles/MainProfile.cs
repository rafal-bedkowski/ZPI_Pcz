using AutoMapper;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Configuration.Profiles;

public class MainProfile : Profile
{
    public MainProfile()
    {
        CreateMap<Product, ProductVm>()
            .ForMember(d => d.Quantity, o => o.MapFrom(s => s.ProductStocks.Sum(ps => ps.Quantity)));
        CreateMap<AddOrUpdateProductVm, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            // Brak obrazu w modelu wejściowym oznacza "bez zmian", nie "usuń".
            .ForMember(d => d.ImageBytes, o => o.Condition(s => s.ImageBytes != null))
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.ProductStocks, o => o.Ignore());

        CreateMap<Address, AddressVm>();
        CreateMap<AddOrUpdateAddressVm, Address>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<StationaryStore, StoreVm>();
        CreateMap<AddOrUpdateStoreVm, StationaryStore>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Address, o => o.Ignore())
            .ForMember(d => d.StationaryStoreEmployee, o => o.Ignore())
            .ForMember(d => d.ProductStock, o => o.Ignore());

        CreateMap<OrderProduct, OrderProductVm>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));
        CreateMap<Order, OrderVm>()
            .ForMember(d => d.InvoiceId, o => o.MapFrom(s => s.Invoice != null ? (int?)s.Invoice.Id : null))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.OrderProducts));

        CreateMap<Invoice, InvoiceVm>();
        CreateMap<AddOrUpdateInvoiceVm, Invoice>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.Order, o => o.Ignore());
    }
}
