using glms.Data;
using glms.Data;
using glms.Models.Entities;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace glms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            var list = await query.ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromForm] Contract contract, IFormFile? file)
        {
            if (file != null)
            {
                if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only PDF files allowed.");
                }

                var folderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "files");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
                var fullPath = Path.Combine(folderPath, uniqueFileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                contract.FilePath = "/files/" + uniqueFileName;
            }

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PatchStatus(int id, [FromBody] JsonElement body)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (!body.TryGetProperty("status", out var statusProp))
                return BadRequest("Missing status");

            var status = statusProp.GetString();
            if (string.IsNullOrEmpty(status)) return BadRequest("Invalid status");

            contract.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
