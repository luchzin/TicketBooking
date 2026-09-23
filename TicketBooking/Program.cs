using System;
using System.Windows.Forms;
using TicketBooking.Controls;
using TicketBooking.Data;
using TicketBooking.Services;

namespace TicketBooking
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure database schema and seed default data
            Database.EnsureCreated();

            // Seed initial administrator and demo customer if they do not exist
            AuthService.CreateUser("085909135", "168168", true, "Super Administrator", "admin@cinebooking.com");
            AuthService.CreateUser("098765432", "123456", false, "Alice Customer", "alice@example.com");

            // Main Application Router Loop
            while (true)
            {
                if (!ProgramState.IsLoggedIn)
                {
                    using (var login = new LoginForm())
                    {
                        var res = login.ShowDialog();
                        if (res != DialogResult.OK || !ProgramState.IsLoggedIn)
                        {
                            break; // User canceled login or exited
                        }
                    }
                }

                // Main Application Window:
                // Admins are routed directly to AdminPortalForm (admins manage the cinema and do not book tickets).
                // Customers are routed to Form1 (movie catalog, showtimes, seating chart & ticket booking).
                if (ProgramState.CurrentUserIsAdmin)
                {
                    Application.Run(new AdminPortalForm());
                }
                else
                {
                    Application.Run(new Form1());
                }

                // If user closed the form without logging out (e.g. window [X] clicked), terminate app
                if (ProgramState.IsLoggedIn)
                {
                    break;
                }
            }
        }
    }
}
