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
            var movieMap = new Dictionary<int, Movie>();

            using (var conn = Database.GetConnection())
            {
                // 1. Fetch all movies in a single query
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Title, Genre, DurationMinutes, Description, PosterPath, Price, Rating, AgeRating, ReleaseDate FROM Movies ORDER BY Id ASC";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            DateTime? relDate = null;
                            if (!r.IsDBNull(9))
                            {
                                DateTime dt;
                                if (DateTime.TryParse(r.GetString(9), out dt))
                                {
                                    relDate = dt;
                                }
                            }

                            var m = new Movie
                            {
                                Id = r.GetInt32(0),
                                Title = r.IsDBNull(1) ? "" : r.GetString(1),
                                Genre = r.IsDBNull(2) ? "" : r.GetString(2),
                                Duration = TimeSpan.FromMinutes(r.IsDBNull(3) ? 0 : r.GetInt32(3)),
                                Description = r.IsDBNull(4) ? "" : r.GetString(4),
                                PosterPath = r.IsDBNull(5) ? "" : r.GetString(5),
                                Price = r.IsDBNull(6) ? 12.00m : Convert.ToDecimal(r.GetDouble(6)),
                                Rating = r.IsDBNull(7) ? "8.5/10" : r.GetString(7),
                                AgeRating = r.IsDBNull(8) ? "PG-13" : r.GetString(8),
                                ReleaseDate = relDate,
                                Shows = new List<Show>()
                            };
                            movies.Add(m);
                            movieMap[m.Id] = m;
                        }
                    }
                }

                // 2. Fetch all shows in a single query
                var allShows = new List<Show>();
                using (var showCmd = conn.CreateCommand())
                {
                    showCmd.CommandText = "SELECT Id, MovieId, ShowTime, HallName, TotalRows, TotalCols FROM Shows ORDER BY ShowTime ASC";
                    using (var sr = showCmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            int showId = sr.GetInt32(0);
                            int mid = sr.GetInt32(1);
                            DateTime time;
                            DateTime.TryParse(sr.GetString(2), out time);
                            string hall = sr.IsDBNull(3) ? "Hall 1" : sr.GetString(3);
                            int rows = sr.IsDBNull(4) ? 6 : sr.GetInt32(4);
                            int cols = sr.IsDBNull(5) ? 8 : sr.GetInt32(5);

                            string title = movieMap.ContainsKey(mid) ? movieMap[mid].Title : "";

                            var show = new Show
                            {
                                Id = showId,
                                MovieId = mid,
                                MovieTitle = title,
                                Time = time,
                                HallName = hall,
                                TotalRows = rows,
                                TotalCols = cols,
                                Seats = new List<Seat>()
                            };

                            allShows.Add(show);
                            if (movieMap.ContainsKey(mid))
                            {
                                movieMap[mid].Shows.Add(show);
                            }
                        }
                    }
                }

                // 3. Fast precompute booked seat counts in a single query
                using (var countCmd = conn.CreateCommand())
                {
                    countCmd.CommandText = "SELECT ShowId, COUNT(*) FROM Bookings WHERE Status = 'Confirmed' GROUP BY ShowId";
                    using (var cr = countCmd.ExecuteReader())
                    {
                        var bookedMap = new Dictionary<int, int>();
                        while (cr.Read())
                        {
                            bookedMap[cr.GetInt32(0)] = cr.GetInt32(1);
                        }

                        foreach (var show in allShows)
                        {
                            int count = bookedMap.ContainsKey(show.Id) ? bookedMap[show.Id] : 0;
                            show.PrecomputedBookedCount = count;
                        }
                    }
                }
            }

            return movies;
        }

        public static void PopulateSeatsForShow(Show show)
        {
            if (show == null) return;
            show.Seats.Clear();
            var bookedSeats = GetBookedSeatCodesForShow(show.Id);

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
            show.PrecomputedBookedCount = show.Seats.FindAll(s => s.IsBooked).Count;
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
                cmd.CommandText = "SELECT SeatCode FROM Bookings WHERE ShowId = $sid AND Status = 'Confirmed'";
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

        public static int AddMovieWithShow(string title, string genre, int durationMinutes, string description, decimal price, DateTime showTime, string hallName, string rating = "8.5/10", string ageRating = "PG-13", DateTime? releaseDate = null, int rows = 6, int cols = 8)
        {
            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                int movieId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = trans;
                    cmd.CommandText = @"
INSERT INTO Movies (Title, Genre, DurationMinutes, Description, PosterPath, Price, Rating, AgeRating, ReleaseDate)
VALUES ($t, $g, $d, $desc, '', $p, $rat, $age, $rd);
SELECT last_insert_rowid();
";
                    cmd.Parameters.AddWithValue("$t", title ?? "");
                    cmd.Parameters.AddWithValue("$g", genre ?? "");
                    cmd.Parameters.AddWithValue("$d", durationMinutes);
                    cmd.Parameters.AddWithValue("$desc", description ?? "");
                    cmd.Parameters.AddWithValue("$p", (double)price);
                    cmd.Parameters.AddWithValue("$rat", rating ?? "8.5/10");
                    cmd.Parameters.AddWithValue("$age", ageRating ?? "PG-13");
                    cmd.Parameters.AddWithValue("$rd", releaseDate.HasValue ? releaseDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                    movieId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var showCmd = conn.CreateCommand())
                {
                    showCmd.Transaction = trans;
                    showCmd.CommandText = @"
INSERT INTO Shows (MovieId, ShowTime, HallName, TotalRows, TotalCols)
VALUES ($mid, $st, $hn, $r, $c);
";
                    showCmd.Parameters.AddWithValue("$mid", movieId);
                    showCmd.Parameters.AddWithValue("$st", showTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    showCmd.Parameters.AddWithValue("$hn", string.IsNullOrEmpty(hallName) ? "Hall 1" : hallName);
                    showCmd.Parameters.AddWithValue("$r", rows);
                    showCmd.Parameters.AddWithValue("$c", cols);
                    showCmd.ExecuteNonQuery();
                }

                trans.Commit();
                return movieId;
            }
        }

        public static bool UpdateMovie(Movie movie)
        {
            if (movie == null || movie.Id <= 0) return false;

            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE Movies
SET Title = $t, Genre = $g, DurationMinutes = $d, Description = $desc, Price = $p, Rating = $rat, AgeRating = $age, ReleaseDate = $rd
WHERE Id = $id;
";
                cmd.Parameters.AddWithValue("$t", movie.Title ?? "");
                cmd.Parameters.AddWithValue("$g", movie.Genre ?? "");
                cmd.Parameters.AddWithValue("$d", (int)movie.Duration.TotalMinutes);
                cmd.Parameters.AddWithValue("$desc", movie.Description ?? "");
                cmd.Parameters.AddWithValue("$p", (double)movie.Price);
                cmd.Parameters.AddWithValue("$rat", movie.Rating ?? "8.5/10");
                cmd.Parameters.AddWithValue("$age", movie.AgeRating ?? "PG-13");
                cmd.Parameters.AddWithValue("$rd", movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("$id", movie.Id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool DeleteMovie(int movieId)
        {
            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Bookings WHERE ShowId IN (SELECT Id FROM Shows WHERE MovieId = $id)";
                        cmd.Parameters.AddWithValue("$id", movieId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Shows WHERE MovieId = $id";
                        cmd.Parameters.AddWithValue("$id", movieId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Movies WHERE Id = $id";
                        cmd.Parameters.AddWithValue("$id", movieId);
                        int count = cmd.ExecuteNonQuery();
                        trans.Commit();
                        return count > 0;
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool DeleteShow(int showId)
        {
            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Bookings WHERE ShowId = $sid";
                        cmd.Parameters.AddWithValue("$sid", showId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Shows WHERE Id = $sid";
                        cmd.Parameters.AddWithValue("$sid", showId);
                        int count = cmd.ExecuteNonQuery();
                        trans.Commit();
                        return count > 0;
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static int AddShowToMovie(int movieId, DateTime showTime, string hallName, int rows = 6, int cols = 8)
        {
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Shows (MovieId, ShowTime, HallName, TotalRows, TotalCols)
VALUES ($mid, $st, $hn, $r, $c);
SELECT last_insert_rowid();
";
                cmd.Parameters.AddWithValue("$mid", movieId);
                cmd.Parameters.AddWithValue("$st", showTime.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$hn", string.IsNullOrEmpty(hallName) ? "Hall 1" : hallName);
                cmd.Parameters.AddWithValue("$r", rows);
                cmd.Parameters.AddWithValue("$c", cols);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
