using Microsoft.AspNetCore.Identity;
using WebStore.Model.DataModels;

namespace WebStore.Tests;

public class ModelTests
{
    private static string[] PropertyNames<T>() =>
        typeof(T).GetProperties().Select(p => p.Name).OrderBy(n => n).ToArray();

    private static string[] Sorted(params string[] names) => names.OrderBy(n => n).ToArray();

    [Fact]
    public void US_01_02_User_InheritsIdentityUserOfInt_AndHasPersonalFields()
    {
        Assert.True(typeof(IdentityUser<int>).IsAssignableFrom(typeof(User)));
        Assert.Equal(typeof(string), typeof(User).GetProperty(nameof(User.FirstName))!.PropertyType);
        Assert.Equal(typeof(string), typeof(User).GetProperty(nameof(User.LastName))!.PropertyType);
        Assert.Equal(typeof(DateTime), typeof(User).GetProperty(nameof(User.RegistrationDate))!.PropertyType);
    }

    [Fact]
    public void US_01_02_SpecializedUsers_InheritUser()
    {
        Assert.Equal(typeof(User), typeof(Customer).BaseType);
        Assert.Equal(typeof(User), typeof(Supplier).BaseType);
        Assert.Equal(typeof(User), typeof(StationaryStoreEmployee).BaseType);
    }

    [Fact]
    public void US_01_02_Supplier_HasProductsList()
    {
        Assert.Equal(typeof(IList<Product>), typeof(Supplier).GetProperty(nameof(Supplier.Products))!.PropertyType);
    }

    [Fact]
    public void US_01_02_Customer_HasOrdersAndSeparateBillingAndShippingAddresses()
    {
        Assert.Equal(typeof(IList<Order>), typeof(Customer).GetProperty(nameof(Customer.Orders))!.PropertyType);
        Assert.Equal(typeof(Address), typeof(Customer).GetProperty(nameof(Customer.BillingAddress))!.PropertyType);
        Assert.Equal(typeof(Address), typeof(Customer).GetProperty(nameof(Customer.ShippingAddress))!.PropertyType);
    }

    [Fact]
    public void US_01_02_Address_HasExactlyTheRequiredFields()
    {
        Assert.Equal(
            Sorted("Id", "Street", "BuildingNumber", "ApartmentNumber", "City", "PostalCode", "Country"),
            PropertyNames<Address>());
    }

    [Fact]
    public void US_01_02_StationaryStoreEmployee_HasStoreIdAndStore()
    {
        Assert.Equal(typeof(int?), typeof(StationaryStoreEmployee).GetProperty("StationaryStoreId")!.PropertyType);
        Assert.Equal(typeof(StationaryStore), typeof(StationaryStoreEmployee).GetProperty("StationaryStore")!.PropertyType);
    }

    [Fact]
    public void US_01_03_Product_HasRequiredFieldsAndLinks()
    {
        Assert.Equal(typeof(int), typeof(Product).GetProperty("Id")!.PropertyType);
        Assert.Equal(typeof(string), typeof(Product).GetProperty("Name")!.PropertyType);
        Assert.Equal(typeof(string), typeof(Product).GetProperty("Description")!.PropertyType);
        Assert.Equal(typeof(byte[]), typeof(Product).GetProperty("ImageBytes")!.PropertyType);
        Assert.Equal(typeof(decimal), typeof(Product).GetProperty("Price")!.PropertyType);
        Assert.Equal(typeof(float), typeof(Product).GetProperty("Weight")!.PropertyType);
        Assert.Equal(typeof(Supplier), typeof(Product).GetProperty("Supplier")!.PropertyType);
        Assert.Equal(typeof(Category), typeof(Product).GetProperty("Category")!.PropertyType);
        Assert.Equal(typeof(int), typeof(Product).GetProperty("CategoryId")!.PropertyType);
        Assert.Equal(typeof(int), typeof(Product).GetProperty("SupplierId")!.PropertyType);
    }

    [Fact]
    public void US_01_03_Category_HasExactlyIdNameTagAndProducts()
    {
        Assert.Equal(Sorted("Id", "Name", "Tag", "Products"), PropertyNames<Category>());
    }

    [Fact]
    public void US_01_03_ProductStock_HasKeyProductStoreAndQuantity()
    {
        Assert.Equal(
            Sorted("Id", "ProductId", "Product", "StationaryStoreId", "StationaryStore", "Quantity"),
            PropertyNames<ProductStock>());
    }

    [Fact]
    public void US_01_04_Order_HasRequiredFields_AndReadOnlyTotalAmount()
    {
        var total = typeof(Order).GetProperty(nameof(Order.TotalAmount))!;

        Assert.Equal(typeof(decimal), total.PropertyType);
        Assert.False(total.CanWrite);
        Assert.Equal(typeof(long), typeof(Order).GetProperty(nameof(Order.TrackingNumber))!.PropertyType);
        Assert.Equal(typeof(Invoice), typeof(Order).GetProperty(nameof(Order.Invoice))!.PropertyType);
        Assert.Equal(typeof(Customer), typeof(Order).GetProperty(nameof(Order.Customer))!.PropertyType);
    }

    [Fact]
    public void US_01_04_OrderProduct_HasLinksQuantityAndFrozenUnitPrice()
    {
        Assert.Equal(
            Sorted("OrderId", "Order", "ProductId", "Product", "Quantity", "UnitPrice"),
            PropertyNames<OrderProduct>());
        Assert.Equal(typeof(decimal), typeof(OrderProduct).GetProperty("UnitPrice")!.PropertyType);
    }

    [Fact]
    public void US_01_04_Invoice_HasExactlyTheRequiredFields()
    {
        Assert.Equal(
            Sorted("Id", "InvoiceNumber", "IssueDate", "PaymentDueDate", "TotalAmount", "OrderId", "Order"),
            PropertyNames<Invoice>());
    }

    [Fact]
    public void US_01_04_TotalAmount_IsSumOfFrozenUnitPriceTimesQuantity()
    {
        var order = new Order();
        order.OrderProducts.Add(new OrderProduct { UnitPrice = 19.99m, Quantity = 3 });
        order.OrderProducts.Add(new OrderProduct { UnitPrice = 0.01m, Quantity = 7 });

        Assert.Equal(60.04m, order.TotalAmount);
    }

    [Fact]
    public void US_01_05_StationaryStore_HasExactlyTheRequiredFields()
    {
        Assert.Equal(
            Sorted("Id", "Name", "AddressId", "Address", "StationaryStoreEmployee", "ProductStock"),
            PropertyNames<StationaryStore>());
    }
}
