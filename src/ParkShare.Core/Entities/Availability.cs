using System;

namespace ParkShare.Core.Entities
{
    public class Availability
    {
        public Guid Id { get; set; }
        public Guid ParkingLotId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; } // Time of day
        public TimeSpan EndTime { get; set; }   // Time of day
        public bool IsAvailable { get; set; } // True if the parking lot is available during this slot
    }
}
