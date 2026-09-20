using System;
using System.Collections.Generic;

namespace TicketBooking.Models
{
    public class Show
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string HallName { get; set; } = "Hall 1";
        public int TotalRows { get; set; } = 6;
        public int TotalCols { get; set; } = 8;
        public List<Seat> Seats { get; set; } = new List<Seat>();

        public int TotalSeats => TotalRows * TotalCols;
        public int Capacity => TotalSeats;
        public int? PrecomputedBookedCount { get; set; }
        public int BookedSeatsCount => PrecomputedBookedCount ?? (Seats != null && Seats.Count > 0 ? Seats.FindAll(s => s.IsBooked).Count : 0);
        public int AvailableSeatsCount => Math.Max(0, TotalSeats - BookedSeatsCount);
        public int RemainingSeatsCount => AvailableSeatsCount;

        public string DisplayText => $"{Time:ddd, MMM d • hh:mm tt} ({HallName})";

        public override string ToString() => DisplayText;
    }
}
