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
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Clients = _context.Clients.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Contract contract, IFormFile file)
    {
        if (file != null)
        {
            // Validate file type
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Only PDF files allowed.");
            }

            // STOP if validation fails
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = _context.Clients.ToList();
                return View(contract);
            }

            // Ensure folder exists
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

            var fullPath = Path.Combine(folderPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save relative path to DB
            contract.FilePath = "/files/" + uniqueFileName;
        }

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}