using glms.Data;
using glms.Interfaces;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(AppDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sr == null) return NotFound();
            return Ok(sr);
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

            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }
    }
}
