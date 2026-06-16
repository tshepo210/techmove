using glms.Data;
using glms.Interfaces;
using glms.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Currency and HTTP clients used by currency service
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddHttpClient<CurrencyService>();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ChangeThisSecretKeyForDevOnly";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "glms";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "glms_clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        // Ensure signing key is at least 256 bits for HS256, derive via SHA256 when too short
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey ?? string.Empty);
        if (keyBytes.Length < 32)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty));
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Prevent JSON serializer from throwing on cyclic navigation properties during API responses
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Ensure database is created and seed a test user if missing
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.EnsureCreated();

        if (!ctx.Users.Any())
        {
            // Seed a default admin user (password: Password123)
            using var sha = System.Security.Cryptography.SHA256.Create();
            var pwd = "Password123";
            var hash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd))).Replace("-", "").ToLowerInvariant();

            ctx.Users.Add(new glms.Models.Entities.User
            {
                Username = "admin",
                PasswordHash = hash,
                Role = "Administrator"
            });

            ctx.SaveChanges();
        }
    }
    catch
    {
        // ignore seeding errors in dev
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
