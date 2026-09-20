using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using BCrypt.Net;
using TicketBooking.Data;
using TicketBooking.Models;

namespace TicketBooking.Services
{
    public static class AuthService
    {
        public static AuthResult Login(LoginRequest request)
        {
            if (request == null) return AuthResult.Fail("Empty request.");

            string phone = request.Phone?.Trim();
            string pass = request.Password;

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass))
            {
                return AuthResult.Fail("Please enter both phone number and password.");
            }

            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Phone, FullName, Email, PasswordHash, IsAdmin, CreatedAt FROM Users WHERE Phone = $p LIMIT 1";
                cmd.Parameters.AddWithValue("$p", phone);

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        return AuthResult.Fail("No account found with this phone number.");
                    }

                    int id = r.GetInt32(0);
                    string dbPhone = r.GetString(1);
                    string fullName = r.IsDBNull(2) ? "" : r.GetString(2);
                    string email = r.IsDBNull(3) ? "" : r.GetString(3);
                    string hash = r.GetString(4);
                    bool isAdmin = r.GetInt32(5) == 1;
                    DateTime createdAt;
                    DateTime.TryParse(r.GetString(6), out createdAt);

                    if (!BCrypt.Net.BCrypt.Verify(pass, hash))
                    {
                        return AuthResult.Fail("Incorrect password. Please try again.");
                    }

                    var user = new User
                    {
                        Id = id,
                        Phone = dbPhone,
                        FullName = fullName,
                        Email = email,
                        IsAdmin = isAdmin,
                        CreatedAt = createdAt
                    };

                    return AuthResult.Ok(user);
                }
            }
        }

        public static AuthResult Register(RegisterRequest request)
        {
            if (request == null) return AuthResult.Fail("Empty registration request.");

            string phone = request.Phone?.Trim();
            string fullName = request.FullName?.Trim() ?? "";
            string email = request.Email?.Trim() ?? "";
            string p1 = request.Password;
            string p2 = request.ConfirmPassword;

            if (string.IsNullOrWhiteSpace(phone))
            {
                return AuthResult.Fail("Phone number is required.");
            }

            if (phone.Length < 6 || !Regex.IsMatch(phone, @"^[0-9+\- ]+$"))
            {
                return AuthResult.Fail("Phone number must contain at least 6 digits.");
            }

            if (string.IsNullOrEmpty(p1) || p1.Length < 4)
            {
                return AuthResult.Fail("Password must be at least 4 characters.");
            }

            if (p1 != p2)
            {
                return AuthResult.Fail("Passwords do not match.");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(p1);

            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Users (Phone, FullName, Email, PasswordHash, IsAdmin, CreatedAt)
VALUES ($p, $fn, $em, $h, $a, $t);
SELECT last_insert_rowid();
";
                cmd.Parameters.AddWithValue("$p", phone);
                cmd.Parameters.AddWithValue("$fn", fullName);
                cmd.Parameters.AddWithValue("$em", email);
                cmd.Parameters.AddWithValue("$h", hash);
                cmd.Parameters.AddWithValue("$a", request.IsAdmin ? 1 : 0);
                cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    var newUser = new User
                    {
                        Id = newId,
                        Phone = phone,
                        FullName = fullName,
                        Email = email,
                        IsAdmin = request.IsAdmin,
                        CreatedAt = DateTime.Now
                    };
                    return AuthResult.Ok(newUser);
                }
                catch (SqliteException ex)
                {
                    if (ex.SqliteErrorCode == 19 || ex.Message.ToLower().Contains("unique"))
                    {
                        return AuthResult.Fail("An account with this phone number already exists.");
                    }
                    return AuthResult.Fail("Database error: " + ex.Message);
                }
            }
        }

        public static List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Phone, FullName, Email, IsAdmin, CreatedAt FROM Users ORDER BY Id ASC";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        DateTime created;
                        DateTime.TryParse(r.IsDBNull(5) ? "" : r.GetString(5), out created);

                        list.Add(new User
                        {
                            Id = r.GetInt32(0),
                            Phone = r.GetString(1),
                            FullName = r.IsDBNull(2) ? "" : r.GetString(2),
                            Email = r.IsDBNull(3) ? "" : r.GetString(3),
                            IsAdmin = r.GetInt32(4) == 1,
                            CreatedAt = created
                        });
                    }
                }
            }
            return list;
        }

        public static AuthResult ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                return AuthResult.Fail("New password must be at least 4 characters.");
            }

            using (var conn = Database.GetConnection())
            {
                // Verify old password
                string currentHash = null;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT PasswordHash FROM Users WHERE Id = $id";
                    cmd.Parameters.AddWithValue("$id", userId);
                    var res = cmd.ExecuteScalar();
                    if (res != null) currentHash = res.ToString();
                }

                if (currentHash == null || !BCrypt.Net.BCrypt.Verify(oldPassword, currentHash))
                {
                    return AuthResult.Fail("Current password is incorrect.");
                }

                // Update hash
                string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                using (var updateCmd = conn.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE Users SET PasswordHash = $h WHERE Id = $id";
                    updateCmd.Parameters.AddWithValue("$h", newHash);
                    updateCmd.Parameters.AddWithValue("$id", userId);
                    updateCmd.ExecuteNonQuery();
                }

                return AuthResult.Ok(null);
            }
        }

        public static AuthResult UpdateProfile(int userId, string fullName, string email)
        {
            fullName = fullName?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return AuthResult.Fail("Please enter a valid email address (e.g., name@example.com).");
            }

            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET FullName = $fn, Email = $em WHERE Id = $id";
                cmd.Parameters.AddWithValue("$fn", fullName);
                cmd.Parameters.AddWithValue("$em", email);
                cmd.Parameters.AddWithValue("$id", userId);
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    if (ProgramState.CurrentUser != null && ProgramState.CurrentUser.Id == userId)
                    {
                        ProgramState.CurrentUser.FullName = fullName;
                        ProgramState.CurrentUser.Email = email;
                    }
                    return AuthResult.Ok(ProgramState.CurrentUser);
                }
                return AuthResult.Fail("User account not found.");
            }
        }

        // Backward compatibility overloads
        public static bool ValidateUser(string phone, string password, out User user)
        {
            var res = Login(new LoginRequest { Phone = phone, Password = password });
            user = res.User;
            return res.Success;
        }

        public static bool ValidateUser(string phone, string password, out bool isAdmin)
        {
            isAdmin = false;
            var res = Login(new LoginRequest { Phone = phone, Password = password });
            if (res.Success && res.User != null)
            {
                isAdmin = res.User.IsAdmin;
                return true;
            }
            return false;
        }

        public static bool UserExists(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 1 FROM Users WHERE Phone = $p LIMIT 1";
                cmd.Parameters.AddWithValue("$p", phone.Trim());
                var res = cmd.ExecuteScalar();
                return res != null && res != DBNull.Value;
            }
        }

        public static bool CreateUser(string phone, string password, bool isAdmin, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (UserExists(phone)) return true;

            var res = Register(new RegisterRequest
            {
                Phone = phone,
                Password = password,
                ConfirmPassword = password,
                IsAdmin = isAdmin
            });
            errorMessage = res.ErrorMessage;
            return res.Success;
        }

        public static bool CreateUser(string phone, string password, bool isAdmin = false)
        {
            string err;
            return CreateUser(phone, password, isAdmin, out err);
        }

        public static bool CreateUser(string phone, string password, bool isAdmin, string fullName, string email)
        {
            if (UserExists(phone)) return true;

            var res = Register(new RegisterRequest
            {
                Phone = phone,
                FullName = fullName,
                Email = email,
                Password = password,
                ConfirmPassword = password,
                IsAdmin = isAdmin
            });
            return res.Success;
        }
    }
}
