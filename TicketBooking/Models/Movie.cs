using System;
using System.Collections.Generic;

namespace TicketBooking.Models
{
    public class Seat
    {
        public string Id { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public bool IsBooked { get; set; }
        public bool IsSelected { get; set; }

        public string Label => $"{(char)('A' + Row)}{Number + 1}";
    }

    public class Show
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public DateTime Time { get; set; }
        public string HallName { get; set; } = "Hall 1";
        public int TotalRows { get; set; } = 6;
        public int TotalCols { get; set; } = 8;
        public List<Seat> Seats { get; set; } = new List<Seat>();

        public string DisplayText => $"{Time:ddd, MMM d • hh:mm tt} ({HallName})";

        public override string ToString() => DisplayText;
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }
        public string PosterPath { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; } = 12.00m;
        public List<Show> Shows { get; set; } = new List<Show>();

        public override string ToString() => Title;
    }

    public class BookingRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ShowId { get; set; }
        public string MovieTitle { get; set; }
        public string HallName { get; set; }
        public DateTime ShowTime { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
        public DateTime BookingTime { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public bool IsAdmin { get; set; }
    }
}
