using glms.Data;
using glms.Interfaces;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("[controller]")]
public class ServiceRequestController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICurrencyService _currencyService;

    public ServiceRequestController(AppDbContext context, ICurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var requests = _context.ServiceRequests
            .Include(r => r.Contract)
            .ThenInclude(c => c.Client)
            .ToList();

        return View(requests);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.Contracts = _context.Contracts.ToList();
        return View();
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        // Validate contract
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId);

        if (contract == null)
        {
            ModelState.AddModelError("", "Contract not found.");
        }
        else if (contract.Status == "Expired" || contract.Status == "On Hold")
        {
            ModelState.AddModelError("",
                "Service Request cannot be created for Expired or On Hold contracts.");
        }

        // Validate model
        if (!ModelState.IsValid)
        {
            ViewBag.Contracts = _context.Contracts.ToList();
            return View(request);
        }

        // Currency conversion
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
            ModelState.AddModelError("", "Currency conversion failed.");
            ViewBag.Contracts = _context.Contracts.ToList();
            return View(request);
        }

        // Save
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}