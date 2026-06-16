using glms.Data;
using glms.Interfaces;
using glms.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// The MVC client no longer talks to the database directly.
// It will call the new Web API. Configure an HttpClient for that and a delegating handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddTransient<glms.Services.ApiAuthHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    // API base address can be configured in appsettings.json under ApiBaseUrl.
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001");
}).AddHttpMessageHandler<glms.Services.ApiAuthHandler>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cookie authentication to represent the logged-in user in the MVC app
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

// Correct registrations (leave currency services as they are).
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
