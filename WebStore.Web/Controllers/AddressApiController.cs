using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Controllers;

public class AddressApiController : BaseApiController
{
    private readonly IAddressService _service;

    public AddressApiController(IAddressService service, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AddressVm>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Execute(() => Ok(_service.GetAddresses()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(AddressVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) => GetOne(() => _service.GetAddress(e => e.Id == id));

    [HttpPost]
    [ProducesResponseType(typeof(AddressVm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] AddOrUpdateAddressVm model) =>
        Create(model.Id, () => _service.AddOrUpdateAddress(model), vm => CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm));

    [HttpPut]
    [ProducesResponseType(typeof(AddressVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromBody] AddOrUpdateAddressVm model) =>
        Update(model.Id, id => _service.GetAddress(e => e.Id == id) is not null, () => _service.AddOrUpdateAddress(model));
}
