using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Controllers;

public class StoreApiController : BaseApiController
{
    private readonly IStoreService _service;

    public StoreApiController(IStoreService service, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StoreVm>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Execute(() => Ok(_service.GetStores()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(StoreVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) => GetOne(() => _service.GetStore(e => e.Id == id));

    [HttpPost]
    [ProducesResponseType(typeof(StoreVm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] AddOrUpdateStoreVm model) =>
        Create(model.Id, () => _service.AddOrUpdateStore(model), vm => CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm));

    [HttpPut]
    [ProducesResponseType(typeof(StoreVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromBody] AddOrUpdateStoreVm model) =>
        Update(model.Id, id => _service.GetStore(e => e.Id == id) is not null, () => _service.AddOrUpdateStore(model));
}
