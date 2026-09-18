using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class MyBookingsForm : Form
    {
        private DataGridView dgvBookings;
        private Label lblSummary;
        private Label lblEmpty;

        public MyBookingsForm()
        {
            Text = "Ticket Booking - My Bookings";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 480);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = $"🎟️ Booking History for {ProgramState.CurrentUserPhone}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(20, 18),
                AutoSize = true
            };

            dgvBookings = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(680, 340),
                BackgroundColor = Color.FromArgb(32, 38, 48),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(50, 60, 75),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 48, 62);
            dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 210, 180);
            dgvBookings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvBookings.ColumnHeadersHeight = 35;

            dgvBookings.DefaultCellStyle.BackColor = Color.FromArgb(32, 38, 48);
            dgvBookings.DefaultCellStyle.ForeColor = Color.White;
            dgvBookings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 130, 115);
            dgvBookings.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvBookings.RowTemplate.Height = 28;

            dgvBookings.Columns.Add("Movie", "Movie Title");
            dgvBookings.Columns.Add("Hall", "Hall");
            dgvBookings.Columns.Add("ShowTime", "Show Time");
            dgvBookings.Columns.Add("Seat", "Seat");
            dgvBookings.Columns.Add("Price", "Price");
            dgvBookings.Columns.Add("BookedAt", "Booked At");

            dgvBookings.Columns["Hall"].FillWeight = 50;
            dgvBookings.Columns["Seat"].FillWeight = 40;
            dgvBookings.Columns["Price"].FillWeight = 50;
            dgvBookings.Columns["ShowTime"].FillWeight = 90;
            dgvBookings.Columns["BookedAt"].FillWeight = 90;

            lblEmpty = new Label
            {
                Text = "No bookings found yet. Select a movie and purchase your tickets!",
                Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 155, 175),
                Location = new Point(20, 180),
                Size = new Size(680, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            lblSummary = new Label
            {
                Location = new Point(20, 420),
                Size = new Size(500, 30),
                ForeColor = Color.FromArgb(200, 215, 235),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(580, 416),
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 54, 70),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblHeader);
            Controls.Add(dgvBookings);
            Controls.Add(lblEmpty);
            Controls.Add(lblSummary);
            Controls.Add(btnClose);

            CancelButton = btnClose;

            LoadBookings();
        }

        private void LoadBookings()
        {
            dgvBookings.Rows.Clear();
            var bookings = BookingService.GetUserBookings(ProgramState.CurrentUserId);

            if (bookings.Count == 0)
            {
                dgvBookings.Visible = false;
                lblEmpty.Visible = true;
                lblSummary.Text = "Total Bookings: 0 tickets";
                return;
            }

            dgvBookings.Visible = true;
            lblEmpty.Visible = false;

            decimal totalSpent = 0;
            foreach (var b in bookings)
            {
                dgvBookings.Rows.Add(
                    b.MovieTitle,
                    b.HallName,
                    b.ShowTime.ToString("ddd, MMM d • hh:mm tt"),
                    b.SeatCode,
                    $"${b.Price:F2}",
                    b.BookingTime.ToString("yyyy-MM-dd HH:mm")
                );
                totalSpent += b.Price;
            }

            lblSummary.Text = $"Total: {bookings.Count} ticket(s) | Total Amount: ${totalSpent:F2}";
        }
    }
}
