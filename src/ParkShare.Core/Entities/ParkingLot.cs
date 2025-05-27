namespace ParkShare.Core.Entities
{
    public class ParkingLot
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; } // Referencing User.Id
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal HourlyRate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    }
}
