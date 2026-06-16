using glms.Interfaces;
using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[Route("[controller]")]
public class ServiceRequestController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICurrencyService _currencyService;

    public ServiceRequestController(IHttpClientFactory httpClientFactory, ICurrencyService currencyService)
    {
        _httpClientFactory = httpClientFactory;
        _currencyService = currencyService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var requests = await client.GetFromJsonAsync<List<ServiceRequest>>("api/servicerequests") ?? new List<ServiceRequest>();
        return View(requests);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var contracts = await client.GetFromJsonAsync<List<Contract>>("api/contracts") ?? new List<Contract>();
        ViewBag.Contracts = contracts;
        return View();
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        // Basic validation by asking API for the contract
        var contractResp = await client.GetAsync($"api/contracts/{request.ContractId}");
        if (!contractResp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Contract not found.");
        }
        else
        {
            var contract = await contractResp.Content.ReadFromJsonAsync<Contract>();
            if (contract != null && (contract.Status == "Expired" || contract.Status == "On Hold"))
            {
                ModelState.AddModelError("", "Service Request cannot be created for Expired or On Hold contracts.");
            }
        }

        if (!ModelState.IsValid)
        {
            var contracts = await client.GetFromJsonAsync<List<Contract>>("api/contracts") ?? new List<Contract>();
            ViewBag.Contracts = contracts;
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
            var contracts = await client.GetFromJsonAsync<List<Contract>>("api/contracts") ?? new List<Contract>();
            ViewBag.Contracts = contracts;
            return View(request);
        }

        var resp = await client.PostAsJsonAsync("api/servicerequests", request);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to create service request via API.");
            var contracts = await client.GetFromJsonAsync<List<Contract>>("api/contracts") ?? new List<Contract>();
            ViewBag.Contracts = contracts;
            return View(request);
        }

        return RedirectToAction(nameof(Index));
    }
}
