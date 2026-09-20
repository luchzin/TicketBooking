using TicketBooking.Models;

namespace TicketBooking
{
    public static class ProgramState
    {
        public static User CurrentUser { get; private set; }
        public static int CurrentUserId => CurrentUser?.Id ?? 0;
        public static string CurrentUserPhone => CurrentUser?.Phone;
        public static string CurrentUserFullName => CurrentUser?.FullName;
        public static string CurrentUserEmail => CurrentUser?.Email;
        public static bool CurrentUserIsAdmin => CurrentUser?.IsAdmin ?? false;
        public static bool IsLoggedIn => CurrentUser != null && CurrentUser.Id > 0;

        public static void SetUser(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
