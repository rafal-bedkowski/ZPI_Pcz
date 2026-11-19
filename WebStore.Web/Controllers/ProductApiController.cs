using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Controllers;

public class ProductApiController : BaseApiController
{
    private readonly IProductService _service;

    public ProductApiController(IProductService service, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductVm>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Execute(() => Ok(_service.GetProducts()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ProductVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) => GetOne(() => _service.GetProduct(e => e.Id == id));

    [HttpPost]
    [ProducesResponseType(typeof(ProductVm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] AddOrUpdateProductVm model) =>
        Create(model.Id, () => _service.AddOrUpdateProduct(model), vm => CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm));

    [HttpPut]
    [ProducesResponseType(typeof(ProductVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromBody] AddOrUpdateProductVm model) =>
        Update(model.Id, id => _service.GetProduct(e => e.Id == id) is not null, () => _service.AddOrUpdateProduct(model));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id) => Execute(() =>
    {
        _service.HideProduct(id);
        return NoContent();
    });
}
