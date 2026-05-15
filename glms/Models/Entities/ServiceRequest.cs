namespace glms.Models.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
    }
}
