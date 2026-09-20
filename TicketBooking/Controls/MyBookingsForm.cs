using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class MyBookingsForm : Form
    {
        private DataGridView dgvBookings;
        private TextBox txtSearch;
        private ComboBox cbFilter;
        private Label lblCount;

        // KPI Ribbon Labels
        private Label lblKpiTotal;
        private Label lblKpiUpcoming;
        private Label lblKpiPast;
        private Label lblKpiCancelled;
        private Label lblKpiSpent;

        // Right Detail Inspector
        private Panel pnlRightDetail;
        private PictureBox picPoster;
        private Label lblDetailTitle;
        private Label lblDetailMeta;
        private Label lblDetailStatus;
        private Label lblDetailCountdown;
        private Label lblDetailSeat;
        private Label lblDetailHall;
        private Label lblDetailShowTime;
        private Label lblDetailRef;
        private Label lblDetailPrice;
        private Button btnCopyRef;
        private Button btnViewReceipt;
        private Button btnCancelBooking;

        private List<Booking> _allBookings = new List<Booking>();
        private List<Booking> _filteredBookings = new List<Booking>();
        private Booking _selectedBooking;

        public MyBookingsForm()
        {
            Text = "🎟️ My CineTicket Reservations & Tickets Hub";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1120, 720);
            MinimumSize = new Size(960, 600);
            BackColor = Color.FromArgb(20, 24, 34);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            this.SetDoubleBuffered(true);

            InitializeCustomUi();
            LoadBookings();
        }

        private void InitializeCustomUi()
        {
            // ================= 1. TOP HEADER & KPI =================
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 118,
                BackColor = Color.FromArgb(24, 30, 42),
                Padding = new Padding(16, 12, 16, 8)
            };

            var lblHeader = new Label
            {
                Text = $"🎟️ Booking History for {ProgramState.CurrentUser?.DisplayName ?? "User"}",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 180),
                Location = new Point(16, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Review your reserved tickets, inspect showtimes, access digital boarding passes, or manage cancellations.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(18, 38),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblHeader);
            pnlTop.Controls.Add(lblSub);

            // KPI Strip
            var pnlKpi = new Panel
            {
                Location = new Point(16, 68),
                Size = new Size(1070, 42),
                BackColor = Color.Transparent
            };

            int kpiX = 0;
            int kpiW = 160;
            lblKpiTotal = CreateKpi(pnlKpi, "🎟️ Total Tickets: 0", Color.FromArgb(100, 180, 255), ref kpiX, kpiW);
            lblKpiUpcoming = CreateKpi(pnlKpi, "🟢 Upcoming: 0", Color.FromArgb(0, 220, 180), ref kpiX, kpiW);
            lblKpiPast = CreateKpi(pnlKpi, "⌛ Past Shows: 0", Color.FromArgb(170, 185, 205), ref kpiX, kpiW);
            lblKpiCancelled = CreateKpi(pnlKpi, "❌ Cancelled: 0", Color.FromArgb(255, 120, 120), ref kpiX, kpiW);
            lblKpiSpent = CreateKpi(pnlKpi, "💰 Total Paid: $0.00", Color.FromArgb(255, 210, 80), ref kpiX, 190);

            pnlTop.Controls.Add(pnlKpi);
            Controls.Add(pnlTop);

            // ================= 2. FILTER BAR =================
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(28, 34, 48),
                Padding = new Padding(16, 6, 16, 6)
            };

            var lblSearch = new Label
            {
                Text = "🔍 Search:",
                Location = new Point(16, 12),
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 200, 225)
            };
            pnlFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(82, 9),
                Size = new Size(240, 26),
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            pnlFilter.Controls.Add(txtSearch);

            var lblFilter = new Label
            {
                Text = "Filter:",
                Location = new Point(340, 12),
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 200, 225)
            };
            pnlFilter.Controls.Add(lblFilter);

            cbFilter = new ComboBox
            {
                Location = new Point(385, 9),
                Size = new Size(180, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            cbFilter.Items.AddRange(new object[] { "All Bookings", "🟢 Upcoming Screenings", "⌛ Past Screenings", "❌ Cancelled / Refunded" });
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilter.Controls.Add(cbFilter);

            lblCount = new Label
            {
                Text = "Showing 0 bookings",
                Location = new Point(585, 12),
                AutoSize = true,
                ForeColor = Color.FromArgb(140, 160, 185)
            };
            pnlFilter.Controls.Add(lblCount);

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(960, 7),
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(38, 48, 64),
                ForeColor = Color.FromArgb(180, 210, 245),
                Font = new Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadBookings();
            pnlFilter.Controls.Add(btnRefresh);

            Controls.Add(pnlFilter);

            // ================= 3. RIGHT DETAIL INSPECTOR PANEL =================
            pnlRightDetail = new Panel
            {
                Dock = DockStyle.Right,
                Width = 340,
                BackColor = Color.FromArgb(24, 28, 38),
                Padding = new Padding(14, 12, 14, 12)
            };
            pnlRightDetail.SetDoubleBuffered(true);

            BuildDetailInspector();
            Controls.Add(pnlRightDetail);

            // ================= 4. MASTER BOOKINGS GRID =================
            var pnlGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 10, 12, 12)
            };

            dgvBookings = CreateStyledGrid();
            dgvBookings.Dock = DockStyle.Fill;
            dgvBookings.Columns.Add("Ref", "Ref Code");
            dgvBookings.Columns.Add("Movie", "Movie Title");
            dgvBookings.Columns.Add("Hall", "Hall");
            dgvBookings.Columns.Add("ShowTime", "Screening Time");
            dgvBookings.Columns.Add("Seat", "Seat");
            dgvBookings.Columns.Add("Price", "Price");
            dgvBookings.Columns.Add("Status", "Status");
            dgvBookings.Columns.Add("Countdown", "Schedule");
            dgvBookings.Columns.Add("BookedAt", "Booked Date");

            dgvBookings.Columns["Ref"].Width = 100;
            dgvBookings.Columns["Movie"].Width = 145;
            dgvBookings.Columns["Hall"].Width = 75;
            dgvBookings.Columns["ShowTime"].Width = 135;
            dgvBookings.Columns["Seat"].Width = 55;
            dgvBookings.Columns["Price"].Width = 65;
            dgvBookings.Columns["Status"].Width = 85;
            dgvBookings.Columns["Countdown"].Width = 110;
            dgvBookings.Columns["BookedAt"].Width = 95;

            dgvBookings.SelectionChanged += DgvBookings_SelectionChanged;
            dgvBookings.CellDoubleClick += (s, e) => BtnViewReceipt_Click(btnViewReceipt, EventArgs.Empty);

            pnlGridContainer.Controls.Add(dgvBookings);
            Controls.Add(pnlGridContainer);
        }

        private void BuildDetailInspector()
        {
            var lblInspectorHeader = new Label
            {
                Text = "🎫 TICKET INSPECTOR",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 180),
                Location = new Point(14, 10),
                AutoSize = true
            };
            pnlRightDetail.Controls.Add(lblInspectorHeader);

            picPoster = new PictureBox
            {
                Location = new Point(14, 38),
                Size = new Size(110, 148),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(16, 20, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlRightDetail.Controls.Add(picPoster);

            lblDetailTitle = new Label
            {
                Text = "Select a ticket to inspect",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(132, 38),
                Size = new Size(190, 48)
            };
            pnlRightDetail.Controls.Add(lblDetailTitle);

            lblDetailMeta = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(132, 88),
                Size = new Size(190, 32)
            };
            pnlRightDetail.Controls.Add(lblDetailMeta);

            lblDetailStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 180),
                Location = new Point(132, 124),
                AutoSize = true
            };
            pnlRightDetail.Controls.Add(lblDetailStatus);

            lblDetailCountdown = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 200, 80),
                Location = new Point(132, 146),
                AutoSize = true
            };
            pnlRightDetail.Controls.Add(lblDetailCountdown);

            // Card with Seat, Hall, Showtime, Price
            var pnlInfoCard = new Panel
            {
                Location = new Point(14, 196),
                Size = new Size(310, 175),
                BackColor = Color.FromArgb(18, 22, 32),
                Padding = new Padding(12)
            };

            lblDetailSeat = new Label
            {
                Text = "💺 Seat: -",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 205, 75),
                Location = new Point(10, 10),
                AutoSize = true
            };
            lblDetailHall = new Label
            {
                Text = "🏛️ Hall: -",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(210, 225, 245),
                Location = new Point(10, 36),
                AutoSize = true
            };
            lblDetailShowTime = new Label
            {
                Text = "🕒 Showtime: -",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(170, 190, 215),
                Location = new Point(10, 60),
                AutoSize = true
            };
            lblDetailPrice = new Label
            {
                Text = "💳 Ticket Price: $0.00",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 180),
                Location = new Point(10, 86),
                AutoSize = true
            };
            lblDetailRef = new Label
            {
                Text = "Ref: -",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(10, 114),
                AutoSize = true
            };

            btnCopyRef = new Button
            {
                Text = "📋 Copy Ref",
                Location = new Point(10, 138),
                Size = new Size(110, 26),
                BackColor = Color.FromArgb(36, 46, 62),
                ForeColor = Color.FromArgb(180, 210, 245),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopyRef.FlatAppearance.BorderSize = 0;
            btnCopyRef.Click += (s, e) =>
            {
                if (_selectedBooking != null && !string.IsNullOrEmpty(_selectedBooking.ReferenceCode))
                {
                    Clipboard.SetText(_selectedBooking.ReferenceCode);
                    MessageBox.Show($"Booking Reference '{_selectedBooking.ReferenceCode}' copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            pnlInfoCard.Controls.Add(lblDetailSeat);
            pnlInfoCard.Controls.Add(lblDetailHall);
            pnlInfoCard.Controls.Add(lblDetailShowTime);
            pnlInfoCard.Controls.Add(lblDetailPrice);
            pnlInfoCard.Controls.Add(lblDetailRef);
            pnlInfoCard.Controls.Add(btnCopyRef);
            pnlRightDetail.Controls.Add(pnlInfoCard);

            // Action Buttons
            btnViewReceipt = new Button
            {
                Text = "🧾 View / Print E-Ticket",
                Location = new Point(14, 385),
                Size = new Size(310, 42),
                BackColor = Color.FromArgb(0, 160, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewReceipt.FlatAppearance.BorderSize = 0;
            btnViewReceipt.Click += BtnViewReceipt_Click;
            pnlRightDetail.Controls.Add(btnViewReceipt);

            btnCancelBooking = new Button
            {
                Text = "❌ Cancel Booking & Refund",
                Location = new Point(14, 436),
                Size = new Size(310, 40),
                BackColor = Color.FromArgb(70, 32, 38),
                ForeColor = Color.FromArgb(255, 160, 160),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelBooking.FlatAppearance.BorderColor = Color.FromArgb(160, 60, 70);
            btnCancelBooking.Click += BtnCancelBooking_Click;
            pnlRightDetail.Controls.Add(btnCancelBooking);

            var btnClose = new Button
            {
                Text = "Close Window",
                Location = new Point(14, 490),
                Size = new Size(310, 38),
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            pnlRightDetail.Controls.Add(btnClose);
        }

        private Label CreateKpi(Panel container, string text, Color color, ref int x, int width)
        {
            var lbl = new Label
            {
                Text = text,
                ForeColor = color,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(x, 10),
                AutoSize = true
            };
            container.Controls.Add(lbl);
            x += width;
            return lbl;
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.FromArgb(24, 28, 38),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(42, 50, 66),
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

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 58);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 210, 180);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 32;

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(26, 32, 44);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 130, 115);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowTemplate.Height = 30;

            dgv.SetDoubleBuffered(true);
            return dgv;
        }

        public void LoadBookings()
        {
            try
            {
                _allBookings = BookingService.GetUserBookings(ProgramState.CurrentUserId);

                // Update KPI Ribbon
                int total = _allBookings.Count;
                int upcoming = _allBookings.Count(b => b.IsActive && !b.IsPast);
                int past = _allBookings.Count(b => b.IsActive && b.IsPast);
                int cancelled = _allBookings.Count(b => b.IsCancelled);
                decimal spent = _allBookings.Where(b => b.IsActive).Sum(b => b.Price);

                lblKpiTotal.Text = $"🎟️ Total Tickets: {total}";
                lblKpiUpcoming.Text = $"🟢 Upcoming: {upcoming}";
                lblKpiPast.Text = $"⌛ Past Shows: {past}";
                lblKpiCancelled.Text = $"❌ Cancelled: {cancelled}";
                lblKpiSpent.Text = $"💰 Total Paid: ${spent:F2}";

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load your bookings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            string q = txtSearch?.Text?.Trim().ToLowerInvariant() ?? "";
            string filterMode = cbFilter?.SelectedItem?.ToString() ?? "All Bookings";

            var list = _allBookings.AsEnumerable();

            if (filterMode.Contains("Upcoming"))
            {
                list = list.Where(b => b.IsActive && !b.IsPast);
            }
            else if (filterMode.Contains("Past"))
            {
                list = list.Where(b => b.IsActive && b.IsPast);
            }
            else if (filterMode.Contains("Cancelled"))
            {
                list = list.Where(b => b.IsCancelled);
            }

            if (!string.IsNullOrEmpty(q))
            {
                list = list.Where(b =>
                    (b.ReferenceCode != null && b.ReferenceCode.ToLowerInvariant().Contains(q)) ||
                    (b.MovieTitle != null && b.MovieTitle.ToLowerInvariant().Contains(q)) ||
                    (b.HallName != null && b.HallName.ToLowerInvariant().Contains(q)) ||
                    (b.SeatCode != null && b.SeatCode.ToLowerInvariant().Contains(q))
                );
            }

            _filteredBookings = list.ToList();

            dgvBookings.Rows.Clear();
            foreach (var b in _filteredBookings)
            {
                int rowIdx = dgvBookings.Rows.Add(
                    b.ReferenceCode,
                    b.MovieTitle,
                    b.HallName,
                    b.ShowTime.ToString("yyyy-MM-dd hh:mm tt"),
                    b.SeatCode,
                    $"${b.Price:F2}",
                    b.IsActive ? "🟢 Confirmed" : "❌ Cancelled",
                    b.ShowCountdown,
                    b.BookingTime.ToString("yyyy-MM-dd HH:mm")
                );

                var row = dgvBookings.Rows[rowIdx];
                if (b.IsCancelled)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(140, 145, 155);
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(255, 110, 110);
                }
                else if (b.IsPast)
                {
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(160, 175, 195);
                }
                else
                {
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(80, 225, 170);
                }
            }

            lblCount.Text = $"Showing {_filteredBookings.Count} of {_allBookings.Count} bookings";

            if (_filteredBookings.Count > 0)
            {
                SelectBooking(_filteredBookings[0]);
            }
            else
            {
                ClearSelection();
            }
        }

        private void DgvBookings_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0) return;
            int idx = dgvBookings.SelectedRows[0].Index;
            if (idx >= 0 && idx < _filteredBookings.Count)
            {
                SelectBooking(_filteredBookings[idx]);
            }
        }

        private void SelectBooking(Booking b)
        {
            _selectedBooking = b;
            lblDetailTitle.Text = b.MovieTitle;
            lblDetailMeta.Text = $"{b.AgeRating} • {b.Genre} • {b.DurationMinutes} min";
            lblDetailStatus.Text = b.IsActive ? "STATUS: 🟢 CONFIRMED" : "STATUS: ❌ CANCELLED";
            lblDetailStatus.ForeColor = b.IsActive ? Color.FromArgb(60, 220, 160) : Color.FromArgb(255, 110, 110);
            lblDetailCountdown.Text = $"Schedule: {b.ShowCountdown}";

            lblDetailSeat.Text = $"💺 Seat: {b.SeatCode} (Row {b.SeatRow + 1})";
            lblDetailHall.Text = $"🏛️ Hall: {b.HallName}";
            lblDetailShowTime.Text = $"🕒 {b.ShowTime:ddd, MMM d, yyyy • hh:mm tt}";
            lblDetailPrice.Text = $"💳 Ticket Price: ${b.Price:F2}";
            lblDetailRef.Text = $"Ref: {b.ReferenceCode}";

            ImageService.LoadImageAsync(b.PosterPath, picPoster, b.MovieTitle);

            btnCancelBooking.Enabled = b.IsActive;
            btnViewReceipt.Enabled = true;
            btnCopyRef.Enabled = true;
        }

        private void ClearSelection()
        {
            _selectedBooking = null;
            picPoster.Image = null;
            lblDetailTitle.Text = "No ticket selected";
            lblDetailMeta.Text = "";
            lblDetailStatus.Text = "";
            lblDetailCountdown.Text = "";
            lblDetailSeat.Text = "💺 Seat: -";
            lblDetailHall.Text = "🏛️ Hall: -";
            lblDetailShowTime.Text = "🕒 Showtime: -";
            lblDetailPrice.Text = "💳 Ticket Price: $0.00";
            lblDetailRef.Text = "Ref: -";

            btnCancelBooking.Enabled = false;
            btnViewReceipt.Enabled = false;
            btnCopyRef.Enabled = false;
        }

        private void BtnViewReceipt_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Please select a booking to view its official e-ticket.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new TicketReceiptForm(_selectedBooking))
            {
                form.ShowDialog(this);
            }
        }

        private void BtnCancelBooking_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null) return;

            if (!_selectedBooking.IsActive)
            {
                MessageBox.Show("This booking is already cancelled.", "Already Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to cancel your booking for '{_selectedBooking.MovieTitle}'?\n\n" +
                $"Seat: {_selectedBooking.SeatCode}\n" +
                $"Screening: {_selectedBooking.ShowTime:ddd, MMM d • hh:mm tt}\n" +
                $"Refund: ${_selectedBooking.Price:F2}\n\n" +
                "This seat will immediately become available for other moviegoers.",
                "Confirm Ticket Cancellation & Refund",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            string error;
            bool ok = BookingService.CancelBooking(_selectedBooking.Id, ProgramState.CurrentUserId, ProgramState.CurrentUserIsAdmin, out error);
            if (ok)
            {
                MessageBox.Show($"Ticket for seat {_selectedBooking.SeatCode} was cancelled successfully. Refund of ${_selectedBooking.Price:F2} processed.",
                    "Cancellation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBookings();
            }
            else
            {
                MessageBox.Show("Cancellation failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
