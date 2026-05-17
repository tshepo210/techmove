using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace glms.Models.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        [ValidateNever]
        public Contract Contract { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal ConvertedCost { get; set; }
        public string Currency { get; set; } // e.g. USD
        public string Status { get; set; } // Pending, Approved, Rejected
    }
}
