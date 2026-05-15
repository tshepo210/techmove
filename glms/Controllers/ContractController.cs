using glms.Data;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ContractController : Controller
{
    private readonly AppDbContext _context;

    public ContractController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string status, DateTime? startDate, DateTime? endDate)
    {
        var contracts = _context.Set<Contract>()
            .Include(c => c.Client)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            contracts = contracts.Where(c => c.Status == status);
        }

        // Filter by start date
        if (startDate.HasValue)
        {
            contracts = contracts.Where(c => c.StartDate >= startDate.Value);
        }

        // Filter by end date
        if (endDate.HasValue)
        {
            contracts = contracts.Where(c => c.EndDate <= endDate.Value);
        }

        return View(contracts.ToList());
    }

    public IActionResult Create()
    {
        ViewBag.Clients = _context.Set<Client>().ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Contract contract, IFormFile file)
    {
        if (file != null)
        {
            var path = Path.Combine("wwwroot/files", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            contract.FilePath = "/files/" + file.FileName;
        }

        _context.Set<Contract>().Add(contract);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}