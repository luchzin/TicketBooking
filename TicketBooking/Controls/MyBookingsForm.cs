using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class MyBookingsForm : Form
    {
        private DataGridView dgvBookings;
        private Label lblSummary;
        private Label lblEmpty;
        private Button btnCancelBooking;
        private Button btnViewReceipt;
        private List<Booking> _currentBookings = new List<Booking>();

        public MyBookingsForm()
        {
            Text = "Ticket Booking - My Bookings & Tickets";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(820, 520);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = $"🎟️ Booking History for {ProgramState.CurrentUser?.DisplayName ?? "User"}",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(20, 18),
                AutoSize = true
            };

            dgvBookings = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(780, 380),
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
            dgvBookings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvBookings.ColumnHeadersHeight = 35;

            dgvBookings.DefaultCellStyle.BackColor = Color.FromArgb(32, 38, 48);
            dgvBookings.DefaultCellStyle.ForeColor = Color.White;
            dgvBookings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 130, 115);
            dgvBookings.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvBookings.RowTemplate.Height = 30;

            dgvBookings.Columns.Add("Ref", "Ref #");
            dgvBookings.Columns.Add("Movie", "Movie Title");
            dgvBookings.Columns.Add("Hall", "Hall");
            dgvBookings.Columns.Add("ShowTime", "Show Time");
            dgvBookings.Columns.Add("Seat", "Seat");
            dgvBookings.Columns.Add("Price", "Price");
            dgvBookings.Columns.Add("Status", "Status");
            dgvBookings.Columns.Add("BookedAt", "Booked At");

            dgvBookings.Columns["Ref"].FillWeight = 65;
            dgvBookings.Columns["Movie"].FillWeight = 110;
            dgvBookings.Columns["Hall"].FillWeight = 45;
            dgvBookings.Columns["Seat"].FillWeight = 35;
            dgvBookings.Columns["Price"].FillWeight = 45;
            dgvBookings.Columns["Status"].FillWeight = 55;
            dgvBookings.Columns["ShowTime"].FillWeight = 85;
            dgvBookings.Columns["BookedAt"].FillWeight = 75;

            lblEmpty = new Label
            {
                Text = "No bookings found yet. Select a movie and purchase your tickets!",
                Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 155, 175),
                Location = new Point(20, 200),
                Size = new Size(780, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            lblSummary = new Label
            {
                Location = new Point(20, 452),
                Size = new Size(380, 30),
                ForeColor = Color.FromArgb(200, 215, 235),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            btnCancelBooking = new Button
            {
                Text = "❌ Cancel Booking",
                Location = new Point(410, 448),
                Size = new Size(130, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(65, 35, 40),
                ForeColor = Color.FromArgb(255, 150, 150),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelBooking.FlatAppearance.BorderColor = Color.FromArgb(180, 70, 70);
            btnCancelBooking.Click += BtnCancelBooking_Click;

            btnViewReceipt = new Button
            {
                Text = "🧾 View Receipt",
                Location = new Point(550, 448),
                Size = new Size(130, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(36, 48, 64),
                ForeColor = Color.FromArgb(140, 200, 255),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewReceipt.FlatAppearance.BorderSize = 0;
            btnViewReceipt.Click += BtnViewReceipt_Click;

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(690, 448),
                Size = new Size(110, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 54, 70),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblHeader);
            Controls.Add(dgvBookings);
            Controls.Add(lblEmpty);
            Controls.Add(lblSummary);
            Controls.Add(btnCancelBooking);
            Controls.Add(btnViewReceipt);
            Controls.Add(btnClose);

            CancelButton = btnClose;

            LoadBookings();
        }

        private void LoadBookings()
        {
            dgvBookings.Rows.Clear();
            _currentBookings = BookingService.GetUserBookings(ProgramState.CurrentUserId);

            if (_currentBookings.Count == 0)
            {
                dgvBookings.Visible = false;
                lblEmpty.Visible = true;
                lblSummary.Text = "Total Bookings: 0 tickets";
                btnCancelBooking.Enabled = false;
                btnViewReceipt.Enabled = false;
                return;
            }

            dgvBookings.Visible = true;
            lblEmpty.Visible = false;
            btnCancelBooking.Enabled = true;
            btnViewReceipt.Enabled = true;

            decimal totalSpent = 0;
            int confirmedCount = 0;

            for (int i = 0; i < _currentBookings.Count; i++)
            {
                var b = _currentBookings[i];
                int rowIdx = dgvBookings.Rows.Add(
                    b.ReferenceCode,
                    b.MovieTitle,
                    b.HallName,
                    b.ShowTime.ToString("ddd, MMM d • hh:mm tt"),
                    b.SeatCode,
                    $"${b.Price:F2}",
                    b.Status,
                    b.BookingTime.ToString("yyyy-MM-dd HH:mm")
                );

                var row = dgvBookings.Rows[rowIdx];
                if (b.IsActive)
                {
                    totalSpent += b.Price;
                    confirmedCount++;
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(100, 230, 190);
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(130, 135, 145);
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(255, 120, 120);
                }
            }

            lblSummary.Text = $"Active: {confirmedCount} ticket(s) | Total: ${totalSpent:F2}";
        }

        private void BtnCancelBooking_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a booking to cancel.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = dgvBookings.SelectedRows[0].Index;
            if (idx < 0 || idx >= _currentBookings.Count) return;

            var booking = _currentBookings[idx];
            if (!booking.IsActive)
            {
                MessageBox.Show("This ticket has already been cancelled.", "Already Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to cancel your booking for '{booking.MovieTitle}'?\n\nSeat: {booking.SeatCode}\nShowtime: {booking.ShowTime:ddd, MMM d • hh:mm tt}\nRefund Amount: ${booking.Price:F2}\n\nThis will immediately free up the seat for other customers.",
                "Confirm Ticket Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            string err;
            bool ok = BookingService.CancelBooking(booking.Id, ProgramState.CurrentUserId, ProgramState.CurrentUserIsAdmin, out err);
            if (ok)
            {
                MessageBox.Show($"Ticket for seat {booking.SeatCode} has been cancelled successfully. Refund of ${booking.Price:F2} processed.", "Booking Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBookings();
            }
            else
            {
                MessageBox.Show("Failed to cancel booking: " + err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewReceipt_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a booking to view receipt.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = dgvBookings.SelectedRows[0].Index;
            if (idx < 0 || idx >= _currentBookings.Count) return;

            var b = _currentBookings[idx];

            string receipt =
                "=========================================\n" +
                "         🎬 CINETICKET CINEMAS          \n" +
                "       OFFICIAL E-TICKET RECEIPT         \n" +
                "=========================================\n\n" +
                $"Reference No : {b.ReferenceCode}\n" +
                $"Customer     : {b.CustomerName} ({b.CustomerPhone})\n" +
                $"Movie        : {b.MovieTitle}\n" +
                $"Cinema Hall  : {b.HallName}\n" +
                $"Showtime     : {b.ShowTime:dddd, MMMM d, yyyy - hh:mm tt}\n" +
                $"Seat Code    : {b.SeatCode}\n" +
                $"Ticket Price : ${b.Price:F2}\n" +
                $"Status       : {b.Status.ToUpperInvariant()}\n" +
                $"Issued At    : {b.BookingTime:yyyy-MM-dd HH:mm:ss}\n\n" +
                "-----------------------------------------\n" +
                "Please present this e-ticket at the entrance.\n" +
                "Thank you for choosing CineTicket!\n" +
                "=========================================";

            MessageBox.Show(receipt, $"Receipt - {b.ReferenceCode}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
