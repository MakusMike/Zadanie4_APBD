using Microsoft.AspNetCore.Mvc;

namespace Zadanie4_APBD;

[ApiController]
[Route("api/warehouses")]
public class WarehouseController : ControllerBase
{
    private IDataService _dataService;

    public WarehouseController(IDataService dataService) 
    {
        _dataService = dataService;
    }

    [HttpPost]
    public async Task<IActionResult> InsertProduct([FromBody] Order order)
    {
        var varr = await _dataService.InsertProduct(order);

        if (varr == -1)
        {
            return NotFound("Wrong Data...");
        }
        else if (varr == -2)
        {
            return NotFound("Order full...");
        }
        else
        {
            return Ok("Idproduct Warehouse: " + varr);
        }
    }
}