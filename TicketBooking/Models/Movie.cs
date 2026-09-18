using System;
using System.Collections.Generic;
using System.Linq;

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
        public DateTime Time { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();
    }

    public class Movie
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }
        public string Description { get; set; }
        public List<Show> Shows { get; set; } = new List<Show>();

        public override string ToString() => Title;
    }

    public static class DummyData
    {
        public static List<Movie> GetMovies()
        {
            var rnd = new Random(42);
            var movies = new List<Movie>
            {
                new Movie
                {
                    Title = "Neon Nights",
                    Genre = "Sci-Fi / Thriller",
                    Duration = TimeSpan.FromMinutes(125),
                    Description = "A neon-lit chase through a futuristic city where memories can be bought and sold.",
                },
                new Movie
                {
                    Title = "The Last Composer",
                    Genre = "Drama / Music",
                    Duration = TimeSpan.FromMinutes(98),
                    Description = "An aging composer rediscovers his voice when a young prodigy arrives.",
                },
                new Movie
                {
                    Title = "Skybound",
                    Genre = "Adventure / Family",
                    Duration = TimeSpan.FromMinutes(105),
                    Description = "Siblings build a backyard airship and discover a hidden floating island.",
                },
                new Movie
                {
                    Title = "Midnight Bakery",
                    Genre = "RomCom",
                    Duration = TimeSpan.FromMinutes(90),
                    Description = "A night-shift baker and a sleep-deprived coder collide over croissants and code.",
                }
            };

            foreach (var m in movies)
            {
                // create 2 shows per movie
                for (int s = 0; s < 2; s++)
                {
                    var show = new Show { Time = DateTime.Today.AddHours(12 + s * 3 + rnd.Next(0,3)) };
                    // create seat grid: 6 rows x 8 seats
                    for (int r = 0; r < 6; r++)
                    {
                        for (int c = 0; c < 8; c++)
                        {
                            var seat = new Seat
                            {
                                Row = r,
                                Number = c,
                                IsBooked = rnd.NextDouble() < 0.12 // some pre-booked
                            };
                            seat.Id = $"R{r}C{c}";
                            show.Seats.Add(seat);
                        }
                    }
                    m.Shows.Add(show);
                }
            }

            return movies;
        }
    }
}
