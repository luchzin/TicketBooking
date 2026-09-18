using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            // ensure database and seed super admin
            TicketBooking.Data.Database.EnsureCreated();
            // create super admin if not exists
            TicketBooking.Services.AuthService.CreateUser("085909135", "168168", true);

            Application.Run(new Form1());
        }
    }
}
