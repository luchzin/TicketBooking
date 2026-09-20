using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TicketBooking.Data;
using TicketBooking.Models;

namespace TicketBooking.Services
{
    public static class BookingService
    {
        public static bool BookSeats(int userId, int showId, List<Seat> seats, decimal price, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (seats == null || seats.Count == 0)
            {
                errorMessage = "No seats selected.";
                return false;
            }

            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    // Check if any of these seats are already booked with Status = 'Confirmed'
                    foreach (var seat in seats)
                    {
                        using (var checkCmd = conn.CreateCommand())
                        {
                            checkCmd.Transaction = trans;
                            checkCmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE ShowId = $sid AND SeatCode = $code AND Status = 'Confirmed'";
                            checkCmd.Parameters.AddWithValue("$sid", showId);
                            checkCmd.Parameters.AddWithValue("$code", seat.Label);
                            long count = (long)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                errorMessage = $"Seat {seat.Label} is already booked by another customer.";
                                trans.Rollback();
                                return false;
                            }
                        }
                    }

                    // Generate a common booking reference code for this batch
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string refCode = $"#CB-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                    foreach (var seat in seats)
                    {
                        using (var insCmd = conn.CreateCommand())
                        {
                            insCmd.Transaction = trans;
                            insCmd.CommandText = @"
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime, Status, ReferenceCode)
VALUES ($uid, $sid, $code, $r, $c, $pr, $bt, 'Confirmed', $ref);
";
                            insCmd.Parameters.AddWithValue("$uid", userId);
                            insCmd.Parameters.AddWithValue("$sid", showId);
                            insCmd.Parameters.AddWithValue("$code", seat.Label);
                            insCmd.Parameters.AddWithValue("$r", seat.Row);
                            insCmd.Parameters.AddWithValue("$c", seat.Number);
                            insCmd.Parameters.AddWithValue("$pr", (double)price);
                            insCmd.Parameters.AddWithValue("$bt", now);
                            insCmd.Parameters.AddWithValue("$ref", refCode);
                            insCmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    errorMessage = ex.Message;
                    return false;
                }
            }
        }

        public static bool CancelBooking(int bookingId, int userId, bool isAdmin, out string errorMessage)
        {
            errorMessage = string.Empty;

            using (var conn = Database.GetConnection())
            {
                int ownerId = 0;
                string currentStatus = null;
                string seatCode = null;

                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT UserId, Status, SeatCode FROM Bookings WHERE Id = $bid";
                    checkCmd.Parameters.AddWithValue("$bid", bookingId);
                    using (var r = checkCmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            errorMessage = "Booking record not found.";
                            return false;
                        }
                        ownerId = r.GetInt32(0);
                        currentStatus = r.GetString(1);
                        seatCode = r.GetString(2);
                    }
                }

                if (!isAdmin && ownerId != userId)
                {
                    errorMessage = "You are not authorized to cancel this booking.";
                    return false;
                }

                if (string.Equals(currentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "This booking has already been cancelled.";
                    return false;
                }

                using (var updateCmd = conn.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE Bookings SET Status = 'Cancelled' WHERE Id = $bid";
                    updateCmd.Parameters.AddWithValue("$bid", bookingId);
                    updateCmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        public static List<Booking> GetUserBookings(int userId)
        {
            var list = new List<Booking>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT b.Id, b.UserId, COALESCE(u.FullName, u.Phone), u.Phone, b.ShowId, m.Title, s.HallName, s.ShowTime,
       b.SeatCode, b.SeatRow, b.SeatCol, b.Price, b.BookingTime, b.Status, b.ReferenceCode
FROM Bookings b
JOIN Shows s ON b.ShowId = s.Id
JOIN Movies m ON s.MovieId = m.Id
JOIN Users u ON b.UserId = u.Id
WHERE b.UserId = $uid
ORDER BY b.BookingTime DESC;
";
                cmd.Parameters.AddWithValue("$uid", userId);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        DateTime showTime;
                        DateTime.TryParse(r.GetString(7), out showTime);
                        DateTime bookingTime;
                        DateTime.TryParse(r.GetString(12), out bookingTime);

                        list.Add(new Booking
                        {
                            Id = r.GetInt32(0),
                            UserId = r.GetInt32(1),
                            CustomerName = r.IsDBNull(2) ? "" : r.GetString(2),
                            CustomerPhone = r.IsDBNull(3) ? "" : r.GetString(3),
                            ShowId = r.GetInt32(4),
                            MovieTitle = r.GetString(5),
                            HallName = r.GetString(6),
                            ShowTime = showTime,
                            SeatCode = r.GetString(8),
                            SeatRow = r.GetInt32(9),
                            SeatCol = r.GetInt32(10),
                            Price = Convert.ToDecimal(r.GetDouble(11)),
                            BookingTime = bookingTime,
                            Status = r.IsDBNull(13) ? "Confirmed" : r.GetString(13),
                            ReferenceCode = r.IsDBNull(14) ? "" : r.GetString(14)
                        });
                    }
                }
            }

            return list;
        }

        public static List<Booking> GetAllBookings()
        {
            var list = new List<Booking>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT b.Id, b.UserId, COALESCE(u.FullName, u.Phone), u.Phone, b.ShowId, m.Title, s.HallName, s.ShowTime,
       b.SeatCode, b.SeatRow, b.SeatCol, b.Price, b.BookingTime, b.Status, b.ReferenceCode
FROM Bookings b
JOIN Shows s ON b.ShowId = s.Id
JOIN Movies m ON s.MovieId = m.Id
JOIN Users u ON b.UserId = u.Id
ORDER BY b.BookingTime DESC;
";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        DateTime showTime;
                        DateTime.TryParse(r.GetString(7), out showTime);
                        DateTime bookingTime;
                        DateTime.TryParse(r.GetString(12), out bookingTime);

                        list.Add(new Booking
                        {
                            Id = r.GetInt32(0),
                            UserId = r.GetInt32(1),
                            CustomerName = r.IsDBNull(2) ? "" : r.GetString(2),
                            CustomerPhone = r.IsDBNull(3) ? "" : r.GetString(3),
                            ShowId = r.GetInt32(4),
                            MovieTitle = r.GetString(5),
                            HallName = r.GetString(6),
                            ShowTime = showTime,
                            SeatCode = r.GetString(8),
                            SeatRow = r.GetInt32(9),
                            SeatCol = r.GetInt32(10),
                            Price = Convert.ToDecimal(r.GetDouble(11)),
                            BookingTime = bookingTime,
                            Status = r.IsDBNull(13) ? "Confirmed" : r.GetString(13),
                            ReferenceCode = r.IsDBNull(14) ? "" : r.GetString(14)
                        });
                    }
                }
            }
            return list;
        }

        public static AdminAnalytics GetAnalytics()
        {
            var stats = new AdminAnalytics();
            using (var conn = Database.GetConnection())
            {
                // Total revenue from confirmed bookings
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COALESCE(SUM(Price), 0) FROM Bookings WHERE Status = 'Confirmed'";
                    stats.TotalRevenue = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // Total confirmed tickets
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE Status = 'Confirmed'";
                    stats.TotalTicketsSold = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Total cancelled bookings
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE Status = 'Cancelled'";
                    stats.CancelledBookingsCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Total movies
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Movies";
                    stats.ActiveMoviesCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Total customers
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE IsAdmin = 0";
                    stats.TotalCustomersCount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return stats;
        }
    }
}
