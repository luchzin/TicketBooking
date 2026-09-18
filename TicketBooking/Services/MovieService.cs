using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TicketBooking.Data;
using TicketBooking.Models;

namespace TicketBooking.Services
{
    public static class MovieService
    {
        public static List<Movie> GetMoviesWithShows()
        {
            var movies = new List<Movie>();

            using (var conn = Database.GetConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Title, Genre, DurationMinutes, Description, PosterPath, Price FROM Movies ORDER BY Id ASC";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var m = new Movie
                            {
                                Id = r.GetInt32(0),
                                Title = r.IsDBNull(1) ? "" : r.GetString(1),
                                Genre = r.IsDBNull(2) ? "" : r.GetString(2),
                                Duration = TimeSpan.FromMinutes(r.IsDBNull(3) ? 0 : r.GetInt32(3)),
                                Description = r.IsDBNull(4) ? "" : r.GetString(4),
                                PosterPath = r.IsDBNull(5) ? "" : r.GetString(5),
                                Price = r.IsDBNull(6) ? 12.00m : Convert.ToDecimal(r.GetDouble(6)),
                                Shows = new List<Show>()
                            };
                            movies.Add(m);
                        }
                    }
                }

                foreach (var m in movies)
                {
                    using (var showCmd = conn.CreateCommand())
                    {
                        showCmd.CommandText = "SELECT Id, MovieId, ShowTime, HallName, TotalRows, TotalCols FROM Shows WHERE MovieId = $mid ORDER BY ShowTime ASC";
                        showCmd.Parameters.AddWithValue("$mid", m.Id);

                        using (var sr = showCmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                int showId = sr.GetInt32(0);
                                DateTime time;
                                DateTime.TryParse(sr.GetString(2), out time);
                                string hall = sr.IsDBNull(3) ? "Hall 1" : sr.GetString(3);
                                int rows = sr.IsDBNull(4) ? 6 : sr.GetInt32(4);
                                int cols = sr.IsDBNull(5) ? 8 : sr.GetInt32(5);

                                var show = new Show
                                {
                                    Id = showId,
                                    MovieId = m.Id,
                                    Time = time,
                                    HallName = hall,
                                    TotalRows = rows,
                                    TotalCols = cols,
                                    Seats = new List<Seat>()
                                };

                                m.Shows.Add(show);
                            }
                        }
                    }

                    // Populate seats and booked status for each show
                    foreach (var show in m.Shows)
                    {
                        var bookedSeats = GetBookedSeatCodesForShowInternal(conn, show.Id);

                        for (int r = 0; r < show.TotalRows; r++)
                        {
                            for (int c = 0; c < show.TotalCols; c++)
                            {
                                var seat = new Seat
                                {
                                    Row = r,
                                    Number = c,
                                    Id = $"R{r}C{c}"
                                };
                                if (bookedSeats.Contains(seat.Label))
                                {
                                    seat.IsBooked = true;
                                }
                                show.Seats.Add(seat);
                            }
                        }
                    }
                }
            }

            return movies;
        }

        public static HashSet<string> GetBookedSeatCodesForShow(int showId)
        {
            using (var conn = Database.GetConnection())
            {
                return GetBookedSeatCodesForShowInternal(conn, showId);
            }
        }

        private static HashSet<string> GetBookedSeatCodesForShowInternal(SqliteConnection conn, int showId)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT SeatCode FROM Bookings WHERE ShowId = $sid";
                cmd.Parameters.AddWithValue("$sid", showId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        set.Add(r.GetString(0));
                    }
                }
            }
            return set;
        }

        public static int AddMovieWithShow(string title, string genre, int durationMinutes, string description, decimal price, DateTime showTime, string hallName)
        {
            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                int movieId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = trans;
                    cmd.CommandText = @"
INSERT INTO Movies (Title, Genre, DurationMinutes, Description, PosterPath, Price)
VALUES ($t, $g, $d, $desc, '', $p);
SELECT last_insert_rowid();
";
                    cmd.Parameters.AddWithValue("$t", title ?? "");
                    cmd.Parameters.AddWithValue("$g", genre ?? "");
                    cmd.Parameters.AddWithValue("$d", durationMinutes);
                    cmd.Parameters.AddWithValue("$desc", description ?? "");
                    cmd.Parameters.AddWithValue("$p", (double)price);
                    movieId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var showCmd = conn.CreateCommand())
                {
                    showCmd.Transaction = trans;
                    showCmd.CommandText = @"
INSERT INTO Shows (MovieId, ShowTime, HallName, TotalRows, TotalCols)
VALUES ($mid, $st, $hn, 6, 8);
";
                    showCmd.Parameters.AddWithValue("$mid", movieId);
                    showCmd.Parameters.AddWithValue("$st", showTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    showCmd.Parameters.AddWithValue("$hn", string.IsNullOrEmpty(hallName) ? "Hall 1" : hallName);
                    showCmd.ExecuteNonQuery();
                }

                trans.Commit();
                return movieId;
            }
        }
    }
}
