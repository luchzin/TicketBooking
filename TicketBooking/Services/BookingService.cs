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
                    // Check if any of these seats are already booked
                    foreach (var seat in seats)
                    {
                        using (var checkCmd = conn.CreateCommand())
                        {
                            checkCmd.Transaction = trans;
                            checkCmd.CommandText = "SELECT COUNT(*) FROM Bookings WHERE ShowId = $sid AND SeatCode = $code";
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

                    // Insert bookings
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (var seat in seats)
                    {
                        using (var insCmd = conn.CreateCommand())
                        {
                            insCmd.Transaction = trans;
                            insCmd.CommandText = @"
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime)
VALUES ($uid, $sid, $code, $r, $c, $pr, $bt);
";
                            insCmd.Parameters.AddWithValue("$uid", userId);
                            insCmd.Parameters.AddWithValue("$sid", showId);
                            insCmd.Parameters.AddWithValue("$code", seat.Label);
                            insCmd.Parameters.AddWithValue("$r", seat.Row);
                            insCmd.Parameters.AddWithValue("$c", seat.Number);
                            insCmd.Parameters.AddWithValue("$pr", (double)price);
                            insCmd.Parameters.AddWithValue("$bt", now);
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

        public static List<BookingRecord> GetUserBookings(int userId)
        {
            var list = new List<BookingRecord>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT b.Id, b.UserId, b.ShowId, m.Title, s.HallName, s.ShowTime, b.SeatCode, b.Price, b.BookingTime
FROM Bookings b
JOIN Shows s ON b.ShowId = s.Id
JOIN Movies m ON s.MovieId = m.Id
WHERE b.UserId = $uid
ORDER BY b.BookingTime DESC;
";
                cmd.Parameters.AddWithValue("$uid", userId);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        DateTime showTime;
                        DateTime.TryParse(r.GetString(5), out showTime);
                        DateTime bookingTime;
                        DateTime.TryParse(r.GetString(8), out bookingTime);

                        list.Add(new BookingRecord
                        {
                            Id = r.GetInt32(0),
                            UserId = r.GetInt32(1),
                            ShowId = r.GetInt32(2),
                            MovieTitle = r.GetString(3),
                            HallName = r.GetString(4),
                            ShowTime = showTime,
                            SeatCode = r.GetString(6),
                            Price = Convert.ToDecimal(r.GetDouble(7)),
                            BookingTime = bookingTime
                        });
                    }
                }
            }

            return list;
        }
    }
}
