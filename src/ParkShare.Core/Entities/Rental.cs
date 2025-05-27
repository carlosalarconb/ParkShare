using ParkShare.Core.Enums; // Assuming Enums folder for RentalStatus
using System;

namespace ParkShare.Core.Entities
{
    public class Rental
    {
        public Guid Id { get; set; }
        public Guid ParkingLotId { get; set; }
        public Guid RenterId { get; set; } // Referencing User.Id
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public decimal TotalCost { get; set; }
        public RentalStatus Status { get; set; }
        public DateTime BookedAtUtc { get; set; }
    }
}
