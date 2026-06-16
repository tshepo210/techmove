using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace glms.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<glms.Api.Program>>
    {
        private readonly WebApplicationFactory<glms.Api.Program> _factory;

        public IntegrationTests(WebApplicationFactory<glms.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace AppDbContext with InMemory for testing
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<glms.Data.AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<glms.Data.AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Build service provider to seed data
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<glms.Data.AppDbContext>();
                    db.Database.EnsureCreated();

                    // Seed user
                    if (!db.Users.Any())
                    {
                        using var sha = System.Security.Cryptography.SHA256.Create();
                        var pwd = "Password123";
                        var hash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd))).Replace("-", "").ToLowerInvariant();

                        db.Users.Add(new glms.Models.Entities.User
                        {
                            Username = "admin",
                            PasswordHash = hash,
                            Role = "Administrator"
                        });

                        // Seed a client and contract
                        var client = new glms.Models.Entities.Client
                        {
                            Name = "Test Client",
                            Email = "test@example.com",
                            PhoneNumber = "+1234567890",
                            Region = "TestRegion"
                        };
                        db.Clients.Add(client);
                        db.SaveChanges();

                        var contract = new glms.Models.Entities.Contract
                        {
                            ClientId = client.Id,
                            StartDate = DateTime.UtcNow.AddDays(-10),
                            EndDate = DateTime.UtcNow.AddDays(10),
                            Status = "Draft",
                            ServiceLevel = "Standard"
                        };
                        db.Contracts.Add(contract);

                        db.SaveChanges();
                    }
                });
            });
        }

        [Fact]
        public async Task GetContracts_Returns200()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/contracts");
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(json));
        }

        [Fact]
        public async Task ProtectedEndpoint_RequiresAuth_And_AllowsWithToken()
        {
            var client = _factory.CreateClient();

            // obtain token
            var tokenResp = await client.PostAsJsonAsync("/api/auth/token", new { Username = "admin", Password = "Password123" });
            tokenResp.EnsureSuccessStatusCode();
            var obj = JsonSerializer.Deserialize<JsonElement>(await tokenResp.Content.ReadAsStringAsync());
            var token = obj.GetProperty("access_token").GetString();
            Assert.False(string.IsNullOrEmpty(token));

            // discover an existing contract id
            var contractsResp = await client.GetAsync("/api/contracts");
            contractsResp.EnsureSuccessStatusCode();
            var contractsJson = await contractsResp.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<JsonElement>(contractsJson);
            var firstId = contracts[0].GetProperty("id").GetInt32();

            // Try patch without token - should be 401 or 403
            var patchRespNoAuth = await client.PatchAsync($"/api/contracts/{firstId}/status", JsonContent.Create(new { status = "Active" }));
            Assert.True(patchRespNoAuth.StatusCode == System.Net.HttpStatusCode.Unauthorized || patchRespNoAuth.StatusCode == System.Net.HttpStatusCode.Forbidden);

            // Call with token
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var patchResp = await client.PatchAsync($"/api/contracts/{firstId}/status", JsonContent.Create(new { status = "Active" }));
            Assert.Equal(System.Net.HttpStatusCode.NoContent, patchResp.StatusCode);
        }
    }
}
