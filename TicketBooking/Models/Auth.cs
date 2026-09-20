using System;

namespace TicketBooking.Models
{
    public enum UserRole
    {
        Customer = 0,
        Admin = 1
    }

    public class User
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public UserRole Role => IsAdmin ? UserRole.Admin : UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Phone : $"{FullName} ({Phone})";

        public override string ToString() => DisplayName;
    }

    public class LoginRequest
    {
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool IsAdmin { get; set; } = false;
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public User User { get; set; }

        public static AuthResult Ok(User user) => new AuthResult { Success = true, User = user };
        public static AuthResult Fail(string error) => new AuthResult { Success = false, ErrorMessage = error };
    }
}
