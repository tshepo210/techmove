using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace glms.Controllers
{
    public class ContractController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ContractController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string status, DateTime? startDate, DateTime? endDate)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var url = $"api/contracts?status={status}&startDate={(startDate?.ToString("o") ?? string.Empty)}&endDate={(endDate?.ToString("o") ?? string.Empty)}";

            var contracts = await client.GetFromJsonAsync<List<Contract>>(url) ?? new List<Contract>();

            return View(contracts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var clients = await client.GetFromJsonAsync<List<Models.Entities.Client>>("api/clients") ?? new List<Models.Entities.Client>();
            ViewBag.Clients = clients;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contract contract, IFormFile file)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(contract.ClientId.ToString()), nameof(contract.ClientId));
            form.Add(new StringContent(contract.StartDate.ToString("o")), nameof(contract.StartDate));
            form.Add(new StringContent(contract.EndDate.ToString("o")), nameof(contract.EndDate));
            form.Add(new StringContent(contract.Status ?? string.Empty), nameof(contract.Status));
            form.Add(new StringContent(contract.ServiceLevel ?? string.Empty), nameof(contract.ServiceLevel));

            if (file != null)
            {
                if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "Only PDF files allowed.");
                }

                if (!ModelState.IsValid)
                {
                    // re-fetch clients
                    var clients = await client.GetFromJsonAsync<List<Models.Entities.Client>>("api/clients") ?? new List<Models.Entities.Client>();
                    ViewBag.Clients = clients;
                    return View(contract);
                }

                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/pdf");
                form.Add(streamContent, "file", file.FileName);
            }

            var response = await client.PostAsync("api/contracts", form);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Not authenticated/authorized - redirect to login so user can obtain a token
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Create", "Contract") });
            }

            if (!response.IsSuccessStatusCode)
            {
                var msg = string.Empty;
                try { msg = await response.Content.ReadAsStringAsync(); } catch { }
                ModelState.AddModelError("", string.IsNullOrEmpty(msg) ? "Failed to create contract via API." : $"Failed to create contract via API: {msg}");
                var clients = await client.GetFromJsonAsync<List<Models.Entities.Client>>("api/clients") ?? new List<Models.Entities.Client>();
                ViewBag.Clients = clients;
                return View(contract);
            }

            return RedirectToAction("Index");
        }
    }
}
