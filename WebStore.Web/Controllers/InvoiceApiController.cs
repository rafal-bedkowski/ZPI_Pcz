using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStore.Services.Exceptions;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Controllers;

public class InvoiceApiController : BaseApiController
{
    private readonly IInvoiceService _service;

    public InvoiceApiController(IInvoiceService service, ILogger logger, IMapper mapper) : base(logger, mapper)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvoiceVm>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Execute(() => Ok(_service.GetInvoices()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(InvoiceVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) => GetOne(() => _service.GetInvoice(e => e.Id == id));

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceVm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] AddOrUpdateInvoiceVm model) =>
        Create(model.Id, () => _service.AddOrUpdateInvoice(model),
            vm => CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm), DuplicateInvoiceToConflict);

    [HttpPut]
    [ProducesResponseType(typeof(InvoiceVm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put([FromBody] AddOrUpdateInvoiceVm model) =>
        Update(model.Id, id => _service.GetInvoice(e => e.Id == id) is not null,
            () => _service.AddOrUpdateInvoice(model), DuplicateInvoiceToConflict);

    private IActionResult? DuplicateInvoiceToConflict(Exception ex) => ex is DuplicateInvoiceException
        ? Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Konflikt")
        : null;
}
