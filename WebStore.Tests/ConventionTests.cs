using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Configuration.Profiles;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class ConventionTests
{
    private static readonly Assembly ServicesAssembly = typeof(BaseService).Assembly;
    private static readonly Assembly ViewModelsAssembly = typeof(ProductVm).Assembly;

    private readonly IMapper _mapper;

    public ConventionTests(IMapper mapper) => _mapper = mapper;

    [Fact]
    public void US_01_08_Services_HaveInterfacesAndConcreteServicesNamespaces()
    {
        Assert.Equal("WebStore.Services.Interfaces", typeof(IProductService).Namespace);
        Assert.Equal("WebStore.Services.ConcreteServices", typeof(ProductService).Namespace);
        Assert.True(Directory.Exists(Path.Combine(Infrastructure.RepositoryPaths.Root, "WebStore.Services", "Interfaces")));
        Assert.True(Directory.Exists(Path.Combine(Infrastructure.RepositoryPaths.Root, "WebStore.Services", "ConcreteServices")));
    }

    [Fact]
    public void US_01_08_ViewModels_LiveInVmNamespaceAndEndWithVm()
    {
        var types = ViewModelsAssembly.GetTypes().Where(t => t.IsPublic).ToList();

        Assert.NotEmpty(types);
        Assert.All(types, t =>
        {
            Assert.Equal("WebStore.ViewModels.VM", t.Namespace);
            Assert.EndsWith("Vm", t.Name);
        });
    }

    [Fact]
    public void US_01_08_ServiceInterfaces_ArePublicAndNamedIxxxService()
    {
        var interfaces = ServicesAssembly.GetTypes().Where(t => t.IsInterface).ToList();

        Assert.Equal(5, interfaces.Count);
        Assert.All(interfaces, t =>
        {
            Assert.True(t.IsPublic);
            Assert.StartsWith("I", t.Name);
            Assert.EndsWith("Service", t.Name);
        });
    }

    [Fact]
    public void US_01_08_BaseService_IsAbstractWithProtectedReadonlyDependencies()
    {
        var type = typeof(BaseService);
        var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single();

        Assert.True(type.IsAbstract);
        Assert.Equal(
            new[] { typeof(ApplicationDbContext), typeof(IMapper), typeof(ILogger) },
            constructor.GetParameters().Select(p => p.ParameterType));

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.Equal(3, fields.Length);
        Assert.All(fields, f =>
        {
            Assert.True(f.IsFamily);
            Assert.True(f.IsInitOnly);
        });
    }

    [Fact]
    public void US_01_08_EveryService_InheritsBaseServiceAndHasInterface()
    {
        var services = ServicesAssembly.GetTypes().Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract).ToList();

        Assert.Equal(5, services.Count);
        Assert.All(services, s =>
        {
            Assert.True(s.IsSubclassOf(typeof(BaseService)));
            Assert.Contains(s.GetInterfaces(), i => i.Name == "I" + s.Name);
        });
    }

    [Fact]
    public void US_01_08_MainProfile_LivesInExpectedNamespace()
    {
        Assert.Equal("WebStore.Services.Configuration.Profiles", typeof(MainProfile).Namespace);
    }

    [Fact]
    public void US_01_08_AutoMapperConfiguration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void US_01_08_AddOrUpdateProductVm_EmptyName_FailsValidation()
    {
        var vm = new AddOrUpdateProductVm { Name = "", Description = "opis" };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(vm, new ValidationContext(vm), results, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddOrUpdateProductVm.Name)));
    }
}
