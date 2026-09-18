using TicketBooking.Models;

namespace TicketBooking
{
    public static class ProgramState
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserPhone { get; set; }
        public static bool CurrentUserIsAdmin { get; set; }
        public static bool IsLoggedIn => CurrentUserId > 0;

        public static void SetUser(User user)
        {
            if (user != null)
            {
                CurrentUserId = user.Id;
                CurrentUserPhone = user.Phone;
                CurrentUserIsAdmin = user.IsAdmin;
            }
            else
            {
                Logout();
            }
        }

        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUserPhone = null;
            CurrentUserIsAdmin = false;
        }
    }
}
