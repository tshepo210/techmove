using glms.Data;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this using directive

public class ServiceRequestController : Controller
{
    private readonly AppDbContext _context;

    public ServiceRequestController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var requests = _context.Set<ServiceRequest>() // Use Set<T>() to access the DbSet
            .Include(r => r.Contract)
            .ThenInclude(c => c.Client)
            .ToList();

        return View(requests);
    }

    public IActionResult Create()
    {
        ViewBag.Contracts = _context.Set<Contract>().ToList(); // Use Set<T>() for Contracts
        return View();
    }

    [HttpPost]
    public IActionResult Create(ServiceRequest request)
    {
        var contract = _context.Set<Contract>().Find(request.ContractId); // Use Set<T>() for Contracts

        if (contract.Status == "Expired" || contract.Status == "On Hold")
        {
            ModelState.AddModelError("", "Cannot create request for inactive contract.");
            ViewBag.Contracts = _context.Set<Contract>().ToList(); // Use Set<T>() for Contracts
            return View(request);
        }

        _context.Set<ServiceRequest>().Add(request); // Use Set<T>() for ServiceRequests
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}