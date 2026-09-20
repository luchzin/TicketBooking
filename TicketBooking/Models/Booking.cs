using System;

namespace TicketBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int ShowId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime ShowTime { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public int SeatRow { get; set; }
        public int SeatCol { get; set; }
        public decimal Price { get; set; }
        public DateTime BookingTime { get; set; }
        public string Status { get; set; } = "Confirmed"; // "Confirmed" or "Cancelled"
        public string ReferenceCode { get; set; } = string.Empty;

        public bool IsActive => string.Equals(Status, "Confirmed", StringComparison.OrdinalIgnoreCase);
        public bool IsCancelled => string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase);

        public override string ToString() => $"{MovieTitle} - Seat {SeatCode} ({Status})";
    }

    // Alias for backward compatibility
    public class BookingRecord : Booking
    {
    }

    public class AdminAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public int ActiveMoviesCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public int CancelledBookingsCount { get; set; }
    }
}
