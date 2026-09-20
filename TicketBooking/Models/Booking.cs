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

        // Enhanced movie details for booking inspection
        public string PosterPath { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string Rating { get; set; } = string.Empty;
        public string AgeRating { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }

        public bool IsActive => string.Equals(Status, "Confirmed", StringComparison.OrdinalIgnoreCase);
        public bool IsCancelled => string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase);
        public bool IsPast => ShowTime < DateTime.Now;

        public string ShowCountdown
        {
            get
            {
                if (IsCancelled) return "Cancelled";
                var diff = ShowTime - DateTime.Now;
                if (diff.TotalMinutes < 0) return "Screening Finished";
                if (diff.TotalDays >= 2) return $"In {(int)diff.TotalDays} days";
                if (diff.TotalDays >= 1) return "Tomorrow";
                if (diff.TotalHours >= 2) return $"Today (in {(int)diff.TotalHours} hrs)";
                if (diff.TotalMinutes >= 1) return $"Starts in {(int)diff.TotalMinutes} min!";
                return "Starting now!";
            }
        }

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
