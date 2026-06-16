using glms.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace glms.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var clients = await client.GetFromJsonAsync<List<Client>>("api/clients") ?? new List<Client>();
            return View(clients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid)
                return View(client);

            var http = _httpClientFactory.CreateClient("ApiClient");
            var resp = await http.PostAsJsonAsync("api/clients", client);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create client via API.");
                return View(client);
            }

            return RedirectToAction("Index");
        }
    }
}
