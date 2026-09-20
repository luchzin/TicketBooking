using System;
using System.Collections.Generic;

namespace TicketBooking.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string PosterPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; } = 12.00m;
        public string Rating { get; set; } = "8.5/10";
        public string AgeRating { get; set; } = "PG-13";
        public DateTime? ReleaseDate { get; set; }
        public List<Show> Shows { get; set; } = new List<Show>();

        public string ReleaseDateFormatted => ReleaseDate.HasValue ? ReleaseDate.Value.ToString("yyyy-MM-dd") : "Now Showing";
        public string CameOutText => ReleaseDate.HasValue ? $"Came out: {ReleaseDate.Value:MMM d, yyyy}" : "Now Showing";

        public override string ToString() => Title;
    }
}
