namespace glms.Models.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } // Draft, Active, Expired, On Hold
        public string ServiceLevel { get; set; }
        public string FilePath { get; set; } // PDF upload
        public List<ServiceRequest>? ServiceRequests { get; set; }
    }
}
