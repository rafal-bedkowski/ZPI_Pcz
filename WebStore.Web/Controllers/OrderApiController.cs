using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Controllers;

public class OrderApiController : BaseApiController
{
    private readonly IOrderService _service;

    public OrderApiController(IOrderService service, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderVm>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Execute(() => Ok(_service.GetOrders()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(OrderVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) => GetOne(() => _service.GetOrder(e => e.Id == id));

    [HttpPost]
    [ProducesResponseType(typeof(OrderVm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] AddOrUpdateOrderVm model) =>
        Create(model.Id, () => _service.AddOrUpdateOrder(model), vm => CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm));

    [HttpPut]
    [ProducesResponseType(typeof(OrderVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromBody] AddOrUpdateOrderVm model) =>
        Update(model.Id, id => _service.GetOrder(e => e.Id == id) is not null, () => _service.AddOrUpdateOrder(model));
}
