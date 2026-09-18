using System;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using BCrypt.Net;
using TicketBooking.Data;
using TicketBooking.Models;

namespace TicketBooking.Services
{
    public static class AuthService
    {
        public static bool CreateUser(string phone, string password, bool isAdmin, out string errorMessage)
        {
            errorMessage = string.Empty;
            phone = phone?.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Phone number is required.";
                return false;
            }

            if (phone.Length < 6 || !Regex.IsMatch(phone, @"^[0-9+\- ]+$"))
            {
                errorMessage = "Phone number must be at least 6 digits.";
                return false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 4)
            {
                errorMessage = "Password must be at least 4 characters.";
                return false;
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Users (Phone, PasswordHash, IsAdmin, CreatedAt) VALUES ($p, $h, $a, $t)";
                cmd.Parameters.AddWithValue("$p", phone);
                cmd.Parameters.AddWithValue("$h", hash);
                cmd.Parameters.AddWithValue("$a", isAdmin ? 1 : 0);
                cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqliteException ex)
                {
                    if (ex.SqliteErrorCode == 19 || ex.Message.ToLower().Contains("unique"))
                    {
                        errorMessage = "An account with this phone number already exists.";
                    }
                    else
                    {
                        errorMessage = ex.Message;
                    }
                    return false;
                }
            }
        }

        public static bool CreateUser(string phone, string password, bool isAdmin = false)
        {
            string err;
            return CreateUser(phone, password, isAdmin, out err);
        }

        public static bool ValidateUser(string phone, string password, out User user)
        {
            user = null;
            phone = phone?.Trim();
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
                return false;

            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Phone, PasswordHash, IsAdmin FROM Users WHERE Phone = $p LIMIT 1";
                cmd.Parameters.AddWithValue("$p", phone);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    int id = r.GetInt32(0);
                    string dbPhone = r.GetString(1);
                    string hash = r.GetString(2);
                    bool isAdmin = r.GetInt32(3) == 1;

                    if (BCrypt.Net.BCrypt.Verify(password, hash))
                    {
                        user = new User
                        {
                            Id = id,
                            Phone = dbPhone,
                            IsAdmin = isAdmin
                        };
                        return true;
                    }
                    return false;
                }
            }
        }

        public static bool ValidateUser(string phone, string password, out bool isAdmin)
        {
            isAdmin = false;
            User u;
            if (ValidateUser(phone, password, out u))
            {
                isAdmin = u.IsAdmin;
                return true;
            }
            return false;
        }
    }
}
