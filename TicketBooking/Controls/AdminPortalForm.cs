using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class AdminPortalForm : Form
    {
        // Top Toolbar Buttons
        private Button btnAddMovie;
        private Button btnAddShow;
        private Button btnEditMovie;
        private Button btnDeleteMovie;
        private Button btnCustomerView;
        private Button btnRefresh;
        private Button btnSignOut;

        // KPI Ribbon Labels
        private Label lblKpiRevenue;
        private Label lblKpiTickets;
        private Label lblKpiMovies;
        private Label lblKpiCustomers;
        private Label lblKpiCancelled;

        // Tabs
        private TabControl tabControl;
        private TabPage tabMovies;
        private TabPage tabBookings;
        private TabPage tabCustomers;

        // Tab 1: Movies & Shows
        private DataGridView dgvMovies;
        private DataGridView dgvShows;
        private Label lblSelectedMovieTitle;
        private Button btnAddShowUnderGrid;
        private Button btnDeleteShowUnderGrid;

        // Tab 2: Bookings & Refunds
        private TextBox txtSearchBookings;
        private ComboBox cbBookingStatusFilter;
        private Label lblBookingCount;
        private DataGridView dgvBookings;
        private Button btnCancelBooking;
        private Button btnPrintReceipt;

        // Tab 3: Customers
        private TextBox txtSearchCustomers;
        private Label lblCustomerCount;
        private DataGridView dgvCustomers;

        // Cached Data
        private List<Movie> _allMovies = new List<Movie>();
        private List<Booking> _allBookings = new List<Booking>();
        private List<User> _allUsers = new List<User>();
        private Movie _selectedMovie;

        public AdminPortalForm()
        {
            this.SetDoubleBuffered(true);
            InitializeCustomUi();
            RefreshAllData();
        }

        private void InitializeCustomUi()
        {
            Text = "CineTicket Cinema Administration & Management Console";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1160, 760);
            MinimumSize = new Size(980, 640);
            BackColor = Color.FromArgb(24, 28, 38);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // ================= 1. TOP HEADER & TOOLBAR =================
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.FromArgb(18, 22, 30),
                Padding = new Padding(14, 8, 14, 6)
            };

            // Title & User badge
            var lblTitle = new Label
            {
                Text = "🎬 CINETICKET CINEMA MANAGER",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                Location = new Point(14, 8),
                AutoSize = true
            };

            string adminName = ProgramState.CurrentUserFullName ?? ProgramState.CurrentUserPhone;
            var lblAdminInfo = new Label
            {
                Text = $"👤 Administrator: {adminName} ({ProgramState.CurrentUserPhone})",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(160, 180, 205),
                Location = new Point(16, 32),
                AutoSize = true
            };

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblAdminInfo);

            // Action Toolbar Flow
            int btnTop = 46;
            int btnH = 30;

            btnAddMovie = CreateToolbarButton("➕ Add Movie & Showtime", Color.FromArgb(0, 140, 120), Color.White, 175, btnTop, btnH);
            btnAddMovie.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAddMovie.Click += BtnAddMovie_Click;

            btnAddShow = CreateToolbarButton("🕒 Add Showtime", Color.FromArgb(90, 60, 150), Color.White, 125, btnTop, btnH);
            btnAddShow.Click += BtnAddShow_Click;

            btnEditMovie = CreateToolbarButton("✏️ Edit Movie", Color.FromArgb(40, 65, 95), Color.White, 105, btnTop, btnH);
            btnEditMovie.Click += BtnEditMovie_Click;

            btnDeleteMovie = CreateToolbarButton("🗑️ Delete Movie", Color.FromArgb(100, 35, 40), Color.FromArgb(255, 200, 200), 115, btnTop, btnH);
            btnDeleteMovie.Click += BtnDeleteMovie_Click;

            btnCustomerView = CreateToolbarButton("🎬 Open Cinema View", Color.FromArgb(30, 95, 80), Color.White, 155, btnTop, btnH);
            btnCustomerView.Click += BtnCustomerView_Click;

            btnRefresh = CreateToolbarButton("🔄 Refresh", Color.FromArgb(45, 52, 68), Color.White, 85, btnTop, btnH);
            btnRefresh.Click += (s, e) => RefreshAllData();

            btnSignOut = CreateToolbarButton("🚪 Sign Out", Color.FromArgb(70, 35, 40), Color.FromArgb(255, 170, 170), 85, btnTop, btnH);
            btnSignOut.Click += BtnSignOut_Click;

            // Arrange toolbar buttons horizontally
            int curX = 14;
            Button[] buttons = { btnAddMovie, btnAddShow, btnEditMovie, btnDeleteMovie, btnCustomerView, btnRefresh, btnSignOut };
            foreach (var btn in buttons)
            {
                btn.Location = new Point(curX, btnTop);
                pnlTop.Controls.Add(btn);
                curX += btn.Width + 8;
            }

            Controls.Add(pnlTop);

            // ================= 2. KPI RIBBON =================
            var pnlKpi = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(28, 34, 46),
                Padding = new Padding(16, 8, 16, 8)
            };

            lblKpiRevenue = CreateKpiLabel("💰 Total Revenue: $0.00", Color.FromArgb(0, 210, 180));
            lblKpiTickets = CreateKpiLabel("🎟️ Tickets Sold: 0", Color.FromArgb(100, 180, 255));
            lblKpiMovies = CreateKpiLabel("🎥 Active Movies: 0", Color.FromArgb(210, 140, 255));
            lblKpiCustomers = CreateKpiLabel("👥 Customers: 0", Color.FromArgb(255, 185, 70));
            lblKpiCancelled = CreateKpiLabel("❌ Cancelled/Refunded: 0", Color.FromArgb(255, 120, 120));

            int kpiX = 16;
            Label[] kpiLabels = { lblKpiRevenue, lblKpiTickets, lblKpiMovies, lblKpiCustomers, lblKpiCancelled };
            foreach (var lbl in kpiLabels)
            {
                lbl.Location = new Point(kpiX, 9);
                pnlKpi.Controls.Add(lbl);
                kpiX += lbl.PreferredWidth + 30;
            }

            Controls.Add(pnlKpi);

            // ================= 3. MAIN TABS =================
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Padding = new Point(14, 6)
            };
            tabControl.SetDoubleBuffered(true);

            tabMovies = new TabPage("🎬 Movies & Scheduled Showtimes") { BackColor = Color.FromArgb(20, 24, 34) };
            tabBookings = new TabPage("📋 All Bookings & Refunds") { BackColor = Color.FromArgb(20, 24, 34) };
            tabCustomers = new TabPage("👥 Customer Accounts") { BackColor = Color.FromArgb(20, 24, 34) };

            tabControl.TabPages.Add(tabMovies);
            tabControl.TabPages.Add(tabBookings);
            tabControl.TabPages.Add(tabCustomers);

            Controls.Add(tabControl);

            BuildMoviesTab();
            BuildBookingsTab();
            BuildCustomersTab();
        }

        private Button CreateToolbarButton(string text, Color bg, Color fg, int width, int top, int height)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Label CreateKpiLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                ForeColor = color,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true
            };
        }

        #region TAB 1: MOVIES & SHOWTIMES
        private void BuildMoviesTab()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 310,
                BackColor = Color.FromArgb(32, 38, 52)
            };
            split.SetDoubleBuffered(true);
            tabMovies.Controls.Add(split);

            // TOP: Movies Grid
            var pnlTopMovies = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 4) };
            split.Panel1.Controls.Add(pnlTopMovies);

            var lblMoviesHeader = new Label
            {
                Text = "🎬 Movie Catalog (Select a movie to view & manage its showtimes):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(210, 225, 245),
                Dock = DockStyle.Top,
                Height = 24
            };
            pnlTopMovies.Controls.Add(lblMoviesHeader);

            dgvMovies = CreateStyledGrid();
            dgvMovies.Dock = DockStyle.Fill;
            dgvMovies.Columns.Add("Id", "ID");
            dgvMovies.Columns.Add("Title", "Movie Title");
            dgvMovies.Columns.Add("ReleaseDate", "Came Out Date");
            dgvMovies.Columns.Add("Genre", "Genre");
            dgvMovies.Columns.Add("Duration", "Duration");
            dgvMovies.Columns.Add("Price", "Ticket Price");
            dgvMovies.Columns.Add("Rating", "Rating");
            dgvMovies.Columns.Add("AgeRating", "Age");
            dgvMovies.Columns.Add("ShowsCount", "Showtimes");

            dgvMovies.Columns["Id"].Width = 45;
            dgvMovies.Columns["Title"].Width = 180;
            dgvMovies.Columns["ReleaseDate"].Width = 110;
            dgvMovies.Columns["Genre"].Width = 140;
            dgvMovies.Columns["Duration"].Width = 85;
            dgvMovies.Columns["Price"].Width = 90;
            dgvMovies.Columns["Rating"].Width = 70;
            dgvMovies.Columns["AgeRating"].Width = 65;
            dgvMovies.Columns["ShowsCount"].Width = 80;

            dgvMovies.SelectionChanged += DgvMovies_SelectionChanged;
            pnlTopMovies.Controls.Add(dgvMovies);
            dgvMovies.BringToFront();

            // BOTTOM: Showtimes Grid
            var pnlBottomShows = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 10) };
            split.Panel2.Controls.Add(pnlBottomShows);

            var pnlShowBar = new Panel { Dock = DockStyle.Top, Height = 36 };

            lblSelectedMovieTitle = new Label
            {
                Text = "🕒 Scheduled Showtimes for Selected Movie:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                Location = new Point(0, 8),
                AutoSize = true
            };
            pnlShowBar.Controls.Add(lblSelectedMovieTitle);

            btnAddShowUnderGrid = new Button
            {
                Text = "➕ Add Showtime to This Movie",
                Location = new Point(480, 4),
                Size = new Size(210, 28),
                BackColor = Color.FromArgb(90, 60, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddShowUnderGrid.FlatAppearance.BorderSize = 0;
            btnAddShowUnderGrid.Click += BtnAddShow_Click;
            pnlShowBar.Controls.Add(btnAddShowUnderGrid);

            btnDeleteShowUnderGrid = new Button
            {
                Text = "🗑️ Remove Showtime",
                Location = new Point(700, 4),
                Size = new Size(150, 28),
                BackColor = Color.FromArgb(90, 35, 40),
                ForeColor = Color.FromArgb(255, 190, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand
            };
            btnDeleteShowUnderGrid.FlatAppearance.BorderSize = 0;
            btnDeleteShowUnderGrid.Click += BtnDeleteShow_Click;
            pnlShowBar.Controls.Add(btnDeleteShowUnderGrid);

            pnlBottomShows.Controls.Add(pnlShowBar);

            dgvShows = CreateStyledGrid();
            dgvShows.Dock = DockStyle.Fill;
            dgvShows.Columns.Add("Id", "Show ID");
            dgvShows.Columns.Add("HallName", "Cinema Hall");
            dgvShows.Columns.Add("Time", "Screening Date & Time");
            dgvShows.Columns.Add("Capacity", "Capacity");
            dgvShows.Columns.Add("Booked", "Booked Tickets");
            dgvShows.Columns.Add("Available", "Available Tickets");

            dgvShows.Columns["Id"].Width = 70;
            dgvShows.Columns["HallName"].Width = 160;
            dgvShows.Columns["Time"].Width = 200;
            dgvShows.Columns["Capacity"].Width = 90;
            dgvShows.Columns["Booked"].Width = 110;
            dgvShows.Columns["Available"].Width = 110;

            pnlBottomShows.Controls.Add(dgvShows);
            dgvShows.BringToFront();
        }
        #endregion

        #region TAB 2: BOOKINGS & REFUNDS
        private void BuildBookingsTab()
        {
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(26, 32, 44),
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblSearch = new Label { Text = "🔍 Search:", ForeColor = Color.FromArgb(200, 215, 235), Location = new Point(12, 14), AutoSize = true };
            pnlTop.Controls.Add(lblSearch);

            txtSearchBookings = new TextBox
            {
                Location = new Point(78, 11),
                Width = 260,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchBookings.TextChanged += (s, e) => FilterBookingsGrid();
            pnlTop.Controls.Add(txtSearchBookings);

            var lblStatus = new Label { Text = "Status:", ForeColor = Color.FromArgb(200, 215, 235), Location = new Point(355, 14), AutoSize = true };
            pnlTop.Controls.Add(lblStatus);

            cbBookingStatusFilter = new ComboBox
            {
                Location = new Point(405, 11),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White
            };
            cbBookingStatusFilter.Items.AddRange(new object[] { "All Statuses", "Confirmed", "Cancelled" });
            cbBookingStatusFilter.SelectedIndex = 0;
            cbBookingStatusFilter.SelectedIndexChanged += (s, e) => FilterBookingsGrid();
            pnlTop.Controls.Add(cbBookingStatusFilter);

            lblBookingCount = new Label { Text = "Showing 0 bookings", ForeColor = Color.FromArgb(140, 160, 185), Location = new Point(565, 14), AutoSize = true };
            pnlTop.Controls.Add(lblBookingCount);

            tabBookings.Controls.Add(pnlTop);

            // Bottom Action Toolbar
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(26, 32, 44),
                Padding = new Padding(12, 8, 12, 8)
            };

            btnCancelBooking = new Button
            {
                Text = "❌ Cancel Booking (Refund & Free Seat)",
                Location = new Point(12, 10),
                Size = new Size(260, 32),
                BackColor = Color.FromArgb(170, 40, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelBooking.FlatAppearance.BorderSize = 0;
            btnCancelBooking.Click += BtnCancelBooking_Click;
            pnlBottom.Controls.Add(btnCancelBooking);

            btnPrintReceipt = new Button
            {
                Text = "🧾 View / Print Receipt",
                Location = new Point(285, 10),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(40, 65, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnPrintReceipt.FlatAppearance.BorderSize = 0;
            btnPrintReceipt.Click += BtnPrintReceipt_Click;
            pnlBottom.Controls.Add(btnPrintReceipt);

            tabBookings.Controls.Add(pnlBottom);

            // Center Bookings Grid
            dgvBookings = CreateStyledGrid();
            dgvBookings.Dock = DockStyle.Fill;
            dgvBookings.Columns.Add("Id", "ID");
            dgvBookings.Columns.Add("RefCode", "Ref #");
            dgvBookings.Columns.Add("Customer", "Customer");
            dgvBookings.Columns.Add("Phone", "Phone");
            dgvBookings.Columns.Add("Movie", "Movie Title");
            dgvBookings.Columns.Add("Hall", "Hall");
            dgvBookings.Columns.Add("ShowTime", "Screening Time");
            dgvBookings.Columns.Add("Seat", "Seat");
            dgvBookings.Columns.Add("Price", "Price");
            dgvBookings.Columns.Add("Status", "Status");
            dgvBookings.Columns.Add("BookingTime", "Booked Date");

            dgvBookings.Columns["Id"].Width = 45;
            dgvBookings.Columns["RefCode"].Width = 140;
            dgvBookings.Columns["Customer"].Width = 130;
            dgvBookings.Columns["Phone"].Width = 110;
            dgvBookings.Columns["Movie"].Width = 150;
            dgvBookings.Columns["Hall"].Width = 100;
            dgvBookings.Columns["ShowTime"].Width = 150;
            dgvBookings.Columns["Seat"].Width = 60;
            dgvBookings.Columns["Price"].Width = 70;
            dgvBookings.Columns["Status"].Width = 90;
            dgvBookings.Columns["BookingTime"].Width = 130;

            tabBookings.Controls.Add(dgvBookings);
            dgvBookings.BringToFront();
        }
        #endregion

        #region TAB 3: CUSTOMER ACCOUNTS
        private void BuildCustomersTab()
        {
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(26, 32, 44),
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblSearch = new Label { Text = "🔍 Search Customer:", ForeColor = Color.FromArgb(200, 215, 235), Location = new Point(12, 14), AutoSize = true };
            pnlTop.Controls.Add(lblSearch);

            txtSearchCustomers = new TextBox
            {
                Location = new Point(145, 11),
                Width = 280,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 44, 58),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchCustomers.TextChanged += (s, e) => FilterCustomersGrid();
            pnlTop.Controls.Add(txtSearchCustomers);

            lblCustomerCount = new Label { Text = "Total Users: 0", ForeColor = Color.FromArgb(140, 160, 185), Location = new Point(445, 14), AutoSize = true };
            pnlTop.Controls.Add(lblCustomerCount);

            tabCustomers.Controls.Add(pnlTop);

            dgvCustomers = CreateStyledGrid();
            dgvCustomers.Dock = DockStyle.Fill;
            dgvCustomers.Columns.Add("Id", "User ID");
            dgvCustomers.Columns.Add("FullName", "Full Name");
            dgvCustomers.Columns.Add("Phone", "Phone Number");
            dgvCustomers.Columns.Add("Email", "Email Address");
            dgvCustomers.Columns.Add("Role", "Role");
            dgvCustomers.Columns.Add("CreatedAt", "Registration Date");
            dgvCustomers.Columns.Add("TotalBookings", "Total Bookings");

            dgvCustomers.Columns["Id"].Width = 60;
            dgvCustomers.Columns["FullName"].Width = 160;
            dgvCustomers.Columns["Phone"].Width = 130;
            dgvCustomers.Columns["Email"].Width = 180;
            dgvCustomers.Columns["Role"].Width = 90;
            dgvCustomers.Columns["CreatedAt"].Width = 120;
            dgvCustomers.Columns["TotalBookings"].Width = 100;

            tabCustomers.Controls.Add(dgvCustomers);
            dgvCustomers.BringToFront();
        }
        #endregion

        #region DATA LOADING & REFRESH
        public void RefreshAllData()
        {
            try
            {
                _allMovies = MovieService.GetMoviesWithShows();
                _allBookings = BookingService.GetAllBookings();
                _allUsers = AuthService.GetAllUsers();
                var analytics = BookingService.GetAnalytics();

                // Update KPI Ribbon
                lblKpiRevenue.Text = $"💰 Revenue: ${analytics.TotalRevenue:N2}";
                lblKpiTickets.Text = $"🎟️ Tickets Sold: {analytics.TotalTicketsSold}";
                lblKpiMovies.Text = $"🎥 Active Movies: {analytics.ActiveMoviesCount}";
                lblKpiCustomers.Text = $"👥 Customers: {analytics.TotalCustomersCount}";
                lblKpiCancelled.Text = $"❌ Cancelled/Refunded: {analytics.CancelledBookingsCount}";

                // Render Movies Grid
                RenderMoviesGrid();

                // Render Bookings Grid
                FilterBookingsGrid();

                // Render Customers Grid
                FilterCustomersGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing cinema data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderMoviesGrid()
        {
            dgvMovies.Rows.Clear();
            foreach (var m in _allMovies)
            {
                dgvMovies.Rows.Add(
                    m.Id,
                    m.Title,
                    m.ReleaseDateFormatted,
                    m.Genre,
                    $"{m.Duration.TotalMinutes} min",
                    $"${m.Price:N2}",
                    m.Rating,
                    m.AgeRating,
                    m.Shows?.Count ?? 0
                );
            }

            if (_allMovies.Count > 0)
            {
                SelectMovie(_allMovies[0]);
            }
            else
            {
                _selectedMovie = null;
                dgvShows.Rows.Clear();
                lblSelectedMovieTitle.Text = "🕒 Scheduled Showtimes for Selected Movie: (None)";
            }
        }

        private void DgvMovies_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMovies.SelectedRows.Count == 0) return;
            int rowIdx = dgvMovies.SelectedRows[0].Index;
            if (rowIdx >= 0 && rowIdx < _allMovies.Count)
            {
                SelectMovie(_allMovies[rowIdx]);
            }
        }

        private void SelectMovie(Movie m)
        {
            _selectedMovie = m;
            lblSelectedMovieTitle.Text = $"🕒 Showtimes for: {m.Title} (Came out: {m.ReleaseDateFormatted})";

            dgvShows.Rows.Clear();
            if (m.Shows == null) return;

            foreach (var s in m.Shows)
            {
                dgvShows.Rows.Add(
                    s.Id,
                    s.HallName,
                    s.Time.ToString("yyyy-MM-dd hh:mm tt"),
                    s.Capacity,
                    s.BookedSeatsCount,
                    s.RemainingSeatsCount
                );
            }
        }

        private void FilterBookingsGrid()
        {
            string q = txtSearchBookings?.Text?.Trim().ToLowerInvariant() ?? "";
            string statusFilter = cbBookingStatusFilter?.SelectedItem?.ToString() ?? "All Statuses";

            var filtered = _allBookings.AsEnumerable();

            if (!string.IsNullOrEmpty(q))
            {
                filtered = filtered.Where(b =>
                    (b.ReferenceCode != null && b.ReferenceCode.ToLowerInvariant().Contains(q)) ||
                    (b.CustomerName != null && b.CustomerName.ToLowerInvariant().Contains(q)) ||
                    (b.CustomerPhone != null && b.CustomerPhone.ToLowerInvariant().Contains(q)) ||
                    (b.MovieTitle != null && b.MovieTitle.ToLowerInvariant().Contains(q)) ||
                    (b.SeatCode != null && b.SeatCode.ToLowerInvariant().Contains(q))
                );
            }

            if (statusFilter == "Confirmed")
            {
                filtered = filtered.Where(b => !b.IsCancelled);
            }
            else if (statusFilter == "Cancelled")
            {
                filtered = filtered.Where(b => b.IsCancelled);
            }

            var list = filtered.ToList();

            dgvBookings.Rows.Clear();
            foreach (var b in list)
            {
                int rowIdx = dgvBookings.Rows.Add(
                    b.Id,
                    b.ReferenceCode,
                    string.IsNullOrEmpty(b.CustomerName) ? b.CustomerPhone : b.CustomerName,
                    b.CustomerPhone,
                    b.MovieTitle,
                    b.HallName,
                    b.ShowTime.ToString("yyyy-MM-dd hh:mm tt"),
                    b.SeatCode,
                    $"${b.Price:N2}",
                    b.Status,
                    b.BookingTime.ToString("yyyy-MM-dd HH:mm")
                );

                if (b.IsCancelled)
                {
                    dgvBookings.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(255, 110, 110);
                }
            }

            lblBookingCount.Text = $"Showing {list.Count} of {_allBookings.Count} bookings";
        }

        private void FilterCustomersGrid()
        {
            string q = txtSearchCustomers?.Text?.Trim().ToLowerInvariant() ?? "";

            var filtered = _allUsers.AsEnumerable();
            if (!string.IsNullOrEmpty(q))
            {
                filtered = filtered.Where(u =>
                    (u.FullName != null && u.FullName.ToLowerInvariant().Contains(q)) ||
                    (u.Phone != null && u.Phone.ToLowerInvariant().Contains(q)) ||
                    (u.Email != null && u.Email.ToLowerInvariant().Contains(q))
                );
            }

            var list = filtered.ToList();

            var userBookingCounts = _allBookings
                .GroupBy(b => b.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            dgvCustomers.Rows.Clear();
            foreach (var u in list)
            {
                int totalBookings = userBookingCounts.ContainsKey(u.Id) ? userBookingCounts[u.Id] : 0;

                int rowIdx = dgvCustomers.Rows.Add(
                    u.Id,
                    u.FullName,
                    u.Phone,
                    u.Email,
                    u.IsAdmin ? "Admin" : "Customer",
                    u.CreatedAt.ToString("yyyy-MM-dd"),
                    totalBookings
                );

                if (u.IsAdmin)
                {
                    dgvCustomers.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(250, 190, 80);
                }
            }

            lblCustomerCount.Text = $"Total Users: {list.Count}";
        }
        #endregion

        #region ACTION HANDLERS
        private void BtnAddMovie_Click(object sender, EventArgs e)
        {
            using (var form = new AddMovieForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshAllData();
                }
            }
        }

        private void BtnEditMovie_Click(object sender, EventArgs e)
        {
            if (_selectedMovie == null)
            {
                MessageBox.Show("Please select a movie to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new EditMovieForm(_selectedMovie))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshAllData();
                }
            }
        }

        private void BtnDeleteMovie_Click(object sender, EventArgs e)
        {
            if (_selectedMovie == null)
            {
                MessageBox.Show("Please select a movie to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedMovie.Title}'?\n\nThis will also remove all scheduled showtimes and bookings for this movie.",
                "Confirm Movie Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                bool deleted = MovieService.DeleteMovie(_selectedMovie.Id);
                if (deleted)
                {
                    MessageBox.Show($"Movie '{_selectedMovie.Title}' deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAllData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete movie: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddShow_Click(object sender, EventArgs e)
        {
            int preselectedId = _selectedMovie?.Id ?? 0;
            using (var form = new AddShowForm(_allMovies, preselectedId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshAllData();
                }
            }
        }

        private void BtnDeleteShow_Click(object sender, EventArgs e)
        {
            if (dgvShows.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a showtime to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int showId = Convert.ToInt32(dgvShows.SelectedRows[0].Cells["Id"].Value);

            var confirm = MessageBox.Show(
                "Cancel and remove this showtime?\nAny customer bookings for this show will also be removed.",
                "Confirm Showtime Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                bool deleted = MovieService.DeleteShow(showId);
                if (deleted)
                {
                    MessageBox.Show("Showtime removed successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAllData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove showtime: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelBooking_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a booking from the list to cancel.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int bookingId = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["Id"].Value);
            var booking = _allBookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null) return;

            if (booking.IsCancelled)
            {
                MessageBox.Show("This booking has already been cancelled.", "Already Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Cancel reservation {booking.ReferenceCode} for customer '{booking.CustomerName}'?\n\nSeat {booking.SeatCode} will be immediately refunded and freed for other customers.",
                "Confirm Cancellation & Refund", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            string error;
            bool ok = BookingService.CancelBooking(booking.Id, ProgramState.CurrentUserId, isAdmin: true, out error);
            if (ok)
            {
                MessageBox.Show($"Booking {booking.ReferenceCode} was cancelled successfully. Seat {booking.SeatCode} has been freed.", "Refund Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshAllData();
            }
            else
            {
                MessageBox.Show("Cancellation failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a booking to view receipt.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int bookingId = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["Id"].Value);
            var booking = _allBookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null) return;

            string receipt =
$@"==================================================
           CINETICKET CINEMA RECEIPT
==================================================
Booking Ref:   {booking.ReferenceCode}
Status:        {booking.Status.ToUpper()}
Date Issued:   {booking.BookingTime:yyyy-MM-dd HH:mm:ss}

Customer:      {booking.CustomerName}
Phone:         {booking.CustomerPhone}
--------------------------------------------------
Movie:         {booking.MovieTitle}
Hall:          {booking.HallName}
Showtime:      {booking.ShowTime:yyyy-MM-dd hh:mm tt}
Seat Number:   {booking.SeatCode}
Ticket Price:  ${booking.Price:N2}
==================================================
Thank you for booking with CineTicket!
==================================================";

            MessageBox.Show(receipt, $"Ticket Receipt - {booking.ReferenceCode}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCustomerView_Click(object sender, EventArgs e)
        {
            Hide();
            using (var customerForm = new Form1(isChildView: true))
            {
                customerForm.ShowDialog(this);
            }
            Show();
            RefreshAllData();
        }

        private void BtnSignOut_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to sign out of the Admin Portal?", "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            ProgramState.Logout();
            Close();
        }
        #endregion

        #region STYLED GRID FACTORY
        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.FromArgb(20, 24, 34),
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(40, 48, 64),
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.SetDoubleBuffered(true);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 40, 56);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 200, 170);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 32;

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(22, 26, 36);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(225, 235, 245);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 140, 120);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.RowTemplate.Height = 28;

            return dgv;
        }
        #endregion
    }
}
