using glms.Data;
using glms.Interfaces;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrencyService _currencyService;

    public ServiceRequestsApiController(AppDbContext context, ICurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId);

        if (contract == null)
            return BadRequest("Contract not found");

        if (contract.Status == "Expired" || contract.Status == "On Hold")
            return BadRequest("Invalid contract status");

        try
        {
            request.ConvertedCost = await _currencyService.ConvertCurrency(
                request.Cost,
                request.Currency,
                "ZAR"
            );
        }
        catch
        {
            return StatusCode(500, "Currency conversion failed");
        }

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(request);
    }
}