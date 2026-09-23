using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Controls;
using TicketBooking.Data;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking
{
    public partial class Form1 : Form
    {
        private List<Movie> _allMovies = new List<Movie>();
        private List<Movie> _filteredMovies = new List<Movie>();
        private Movie _selectedMovie;
        private Show _selectedShow;
        private readonly List<MovieCard> _movieCards = new List<MovieCard>();
        private readonly bool _isChildView;
        private Button btnAdminPortal;
        private Button btnProfile;
        private Button btnEditMovieDetail;
        private Button btnDeleteMovieDetail;
        private Button btnAddShowDetail;
        private PictureBox picDetailPoster;

        public Form1(bool isChildView = false)
        {
            _isChildView = isChildView;
            InitializeComponent();

            // Ensure database is created and seeded
            Database.EnsureCreated();

            // Enable double buffering for smooth, flicker-free rendering
            this.SetDoubleBuffered(true);
            flowMovies.SetDoubleBuffered(true);
            pnlSeats.SetDoubleBuffered(true);

            // Setup Admin Portal & Profile buttons
            SetupAdminPortalButton();
            SetupProfileButton();

            // Clear anchors on dynamic top-bar controls so WinForms doesn't add auto-shift offsets
            lblUserInfo.Anchor = AnchorStyles.None;
            btnLogout.Anchor = AnchorStyles.None;
            btnProfile.Anchor = AnchorStyles.None;
            btnMyBookings.Anchor = AnchorStyles.None;
            btnAddMovie.Anchor = AnchorStyles.None;
            btnAdminPortal.Anchor = AnchorStyles.None;

            // Setup Admin Movie Controls on right panel
            SetupAdminMovieControls();

            // Setup poster display in right panel
            SetupPosterDisplay();

            // Setup custom UI components like the legend
            SetupLegend();

            // Setup interactive user info badge
            lblUserInfo.Cursor = Cursors.Hand;
            lblUserInfo.Click += (s, e) => OpenUserProfile();
            var userTip = new ToolTip();
            userTip.SetToolTip(lblUserInfo, "Click to view your profile and account details");
            btnLogout.Text = "🚪 Sign Out";

            // Wire event handlers
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cbShows.SelectedIndexChanged += CbShows_SelectedIndexChanged;
            btnPurchase.Click += BtnPurchase_Click;
            btnAddMovie.Click += BtnAddMovie_Click;
            btnMyBookings.Click += BtnMyBookings_Click;
            btnLogout.Click += BtnLogout_Click;

            // Authentication check
            if (!ProgramState.IsLoggedIn)
            {
                if (!PerformLogin())
                {
                    Load += (s, e) => Close();
                    return;
                }
            }
            else
            {
                UpdateUserSessionUi();
            }

            InitData();
        }

        private void SetupAdminPortalButton()
        {
            btnAdminPortal = new Button
            {
                Text = _isChildView ? "⬅ Admin Portal" : "⚙️ Admin Portal",
                Size = new Size(135, 32),
                Anchor = AnchorStyles.None,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 160, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnAdminPortal.FlatAppearance.BorderSize = 0;
            btnAdminPortal.Click += BtnAdminPortal_Click;
            topBarPanel.Controls.Add(btnAdminPortal);
        }

        private void BtnAdminPortal_Click(object sender, EventArgs e)
        {
            if (_isChildView)
            {
                Close();
            }
            else
            {
                Hide();
                using (var portal = new AdminPortalForm())
                {
                    portal.ShowDialog(this);
                }
                Show();
                if (!ProgramState.IsLoggedIn)
                {
                    Close();
                }
                else
                {
                    UpdateUserSessionUi();
                    InitData();
                }
            }
        }

        private void SetupPosterDisplay()
        {
            picDetailPoster = new PictureBox
            {
                Location = new Point(26, 14),
                Size = new Size(95, 126),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(24, 30, 42),
                BorderStyle = BorderStyle.FixedSingle
            };
            rightPanel.Controls.Add(picDetailPoster);

            lblTitle.Location = new Point(132, 14);
            lblPriceBadge.Location = new Point(134, 46);
            lblMeta.Location = new Point(134, 70);
            lblDescription.Location = new Point(134, 94);
            lblDescription.MaximumSize = new Size(400, 50);
        }

        private void SetupAdminMovieControls()
        {
            btnEditMovieDetail = new Button
            {
                Text = "✏️ Edit Movie",
                Size = new Size(105, 28),
                Location = new Point(540, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(40, 65, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnEditMovieDetail.FlatAppearance.BorderSize = 0;
            btnEditMovieDetail.Click += BtnEditMovieDetail_Click;
            rightPanel.Controls.Add(btnEditMovieDetail);

            btnDeleteMovieDetail = new Button
            {
                Text = "🗑️ Delete Movie",
                Size = new Size(115, 28),
                Location = new Point(540, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 35, 40),
                ForeColor = Color.FromArgb(255, 200, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnDeleteMovieDetail.FlatAppearance.BorderSize = 0;
            btnDeleteMovieDetail.Click += BtnDeleteMovieDetail_Click;
            rightPanel.Controls.Add(btnDeleteMovieDetail);

            btnAddShowDetail = new Button
            {
                Text = "➕ Add Showtime",
                Size = new Size(135, 29),
                Location = new Point(355, 164),
                BackColor = Color.FromArgb(90, 60, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnAddShowDetail.FlatAppearance.BorderSize = 0;
            btnAddShowDetail.Click += BtnAddShowDetail_Click;
            rightPanel.Controls.Add(btnAddShowDetail);
        }

        private void BtnEditMovieDetail_Click(object sender, EventArgs e)
        {
            if (_selectedMovie == null) return;
            using (var edit = new EditMovieForm(_selectedMovie))
            {
                if (edit.ShowDialog(this) == DialogResult.OK)
                {
                    int id = _selectedMovie.Id;
                    _allMovies = MovieService.GetMoviesWithShows();
                    TxtSearch_TextChanged(txtSearch, EventArgs.Empty);
                    var updated = _allMovies.FirstOrDefault(m => m.Id == id);
                    if (updated != null) SelectMovie(updated);
                }
            }
        }

        private void BtnDeleteMovieDetail_Click(object sender, EventArgs e)
        {
            if (_selectedMovie == null) return;
            var confirm = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedMovie.Title}'?\n\nThis will also remove all scheduled showtimes and bookings for this movie.",
                "Confirm Movie Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                bool deleted = MovieService.DeleteMovie(_selectedMovie.Id);
                if (deleted)
                {
                    MessageBox.Show($"Movie '{_selectedMovie.Title}' deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _allMovies = MovieService.GetMoviesWithShows();
                    TxtSearch_TextChanged(txtSearch, EventArgs.Empty);
                    if (_filteredMovies.Count > 0) SelectMovie(_filteredMovies[0]);
                    else ClearMovieDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete movie: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddShowDetail_Click(object sender, EventArgs e)
        {
            int preselectedId = _selectedMovie?.Id ?? 0;
            using (var addShow = new AddShowForm(_allMovies, preselectedId))
            {
                if (addShow.ShowDialog(this) == DialogResult.OK)
                {
                    int curId = _selectedMovie?.Id ?? 0;
                    _allMovies = MovieService.GetMoviesWithShows();
                    TxtSearch_TextChanged(txtSearch, EventArgs.Empty);
                    var refreshed = _allMovies.FirstOrDefault(m => m.Id == curId);
                    if (refreshed != null) SelectMovie(refreshed);
                }
            }
        }

        private bool PerformLogin()
        {
            using (var login = new LoginForm())
            {
                if (login.ShowDialog(this) == DialogResult.OK && ProgramState.IsLoggedIn)
                {
                    UpdateUserSessionUi();
                    return true;
                }
            }
            return false;
        }

        private void SetupProfileButton()
        {
            btnProfile = new Button
            {
                Text = "👤 Profile",
                Size = new Size(92, 32),
                Anchor = AnchorStyles.None,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(36, 48, 64),
                ForeColor = Color.FromArgb(190, 215, 245),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnProfile.FlatAppearance.BorderSize = 0;
            btnProfile.Click += (s, e) => OpenUserProfile();
            topBarPanel.Controls.Add(btnProfile);

            topBarPanel.Resize += (s, e) => LayoutTopBarButtons();
        }

        private void LayoutTopBarButtons()
        {
            if (topBarPanel == null) return;

            // 1. Position User Info badge on the left side, right after Brand label
            if (lblUserInfo != null && lblBrand != null)
            {
                lblUserInfo.Location = new Point(lblBrand.Right + 16, 17);
                lblUserInfo.BringToFront();
            }

            if (!ProgramState.IsLoggedIn) return;

            // 2. Position action buttons from right to left
            int right = topBarPanel.ClientSize.Width - 16;

            if (btnLogout != null)
            {
                btnLogout.Width = 92;
                btnLogout.Location = new Point(right - btnLogout.Width, 12);
                btnLogout.BringToFront();
                right = btnLogout.Location.X - 8;
            }

            if (btnProfile != null)
            {
                btnProfile.Width = 92;
                btnProfile.Location = new Point(right - btnProfile.Width, 12);
                btnProfile.BringToFront();
                right = btnProfile.Location.X - 8;
            }

            if (btnMyBookings != null)
            {
                btnMyBookings.Width = 125;
                btnMyBookings.Location = new Point(right - btnMyBookings.Width, 12);
                btnMyBookings.BringToFront();
                right = btnMyBookings.Location.X - 8;
            }

            bool isAdmin = ProgramState.CurrentUserIsAdmin;

            if (btnAddMovie != null)
            {
                btnAddMovie.Visible = isAdmin;
                if (isAdmin)
                {
                    btnAddMovie.Width = 115;
                    btnAddMovie.Location = new Point(right - btnAddMovie.Width, 12);
                    btnAddMovie.BringToFront();
                    right = btnAddMovie.Location.X - 8;
                }
            }

            if (btnAdminPortal != null)
            {
                btnAdminPortal.Visible = isAdmin;
                if (isAdmin)
                {
                    btnAdminPortal.Width = 135;
                    btnAdminPortal.Location = new Point(right - btnAdminPortal.Width, 12);
                    btnAdminPortal.BringToFront();
                    right = btnAdminPortal.Location.X - 8;
                }
            }
        }

        private void OpenUserProfile()
        {
            if (!ProgramState.IsLoggedIn)
            {
                PerformLogin();
                return;
            }

            using (var profileForm = new UserProfileForm())
            {
                var res = profileForm.ShowDialog(this);
                if (profileForm.RequestedSignOut || res == DialogResult.Abort)
                {
                    TriggerSignOut();
                }
                else
                {
                    UpdateUserSessionUi();
                }
            }
        }

        private void TriggerSignOut()
        {
            ProgramState.Logout();
            UpdateUserSessionUi();

            if (_isChildView)
            {
                Close();
                return;
            }

            if (!PerformLogin())
            {
                Close();
            }
            else
            {
                InitData();
            }
        }

        private void UpdateUserSessionUi()
        {
            if (ProgramState.IsLoggedIn)
            {
                bool isAdmin = ProgramState.CurrentUserIsAdmin;
                string displayName = string.IsNullOrWhiteSpace(ProgramState.CurrentUserFullName)
                    ? ProgramState.CurrentUserPhone
                    : ProgramState.CurrentUserFullName;

                if (isAdmin)
                {
                    lblUserInfo.Text = $"👑 {displayName} [ADMIN]";
                    lblUserInfo.ForeColor = Color.FromArgb(255, 215, 80);
                    lblUserInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else
                {
                    lblUserInfo.Text = $"👤 {displayName}";
                    lblUserInfo.ForeColor = Color.FromArgb(180, 210, 245);
                    lblUserInfo.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }

                lblUserInfo.Visible = true;
                btnAddMovie.Visible = isAdmin;
                if (btnAdminPortal != null) btnAdminPortal.Visible = isAdmin;
                if (btnProfile != null) btnProfile.Visible = true;
                if (btnEditMovieDetail != null) btnEditMovieDetail.Visible = isAdmin;
                if (btnDeleteMovieDetail != null) btnDeleteMovieDetail.Visible = isAdmin;
                if (btnAddShowDetail != null) btnAddShowDetail.Visible = isAdmin;
                btnMyBookings.Visible = true;
                btnLogout.Visible = true;
            }
            else
            {
                lblUserInfo.Text = "Not signed in";
                lblUserInfo.Visible = false;
                btnAddMovie.Visible = false;
                if (btnAdminPortal != null) btnAdminPortal.Visible = false;
                if (btnProfile != null) btnProfile.Visible = false;
                if (btnEditMovieDetail != null) btnEditMovieDetail.Visible = false;
                if (btnDeleteMovieDetail != null) btnDeleteMovieDetail.Visible = false;
                if (btnAddShowDetail != null) btnAddShowDetail.Visible = false;
                btnMyBookings.Visible = false;
                btnLogout.Visible = false;
            }

            LayoutTopBarButtons();
        }

        private void SetupLegend()
        {
            pnlLegend.Controls.Clear();

            var legendItems = new[]
            {
                new { Label = "Available", Color = Color.FromArgb(36, 46, 62), Border = Color.FromArgb(56, 70, 92) },
                new { Label = "Selected", Color = Color.FromArgb(0, 170, 150), Border = Color.FromArgb(100, 240, 220) },
                new { Label = "Booked", Color = Color.FromArgb(42, 46, 56), Border = Color.FromArgb(55, 60, 72) }
            };

            int left = 14;
            foreach (var item in legendItems)
            {
                var swatch = new Panel
                {
                    Location = new Point(left, 4),
                    Size = new Size(16, 16),
                    BackColor = item.Color
                };
                swatch.Paint += (s, e) =>
                {
                    using (var p = new Pen(item.Border, 1))
                    {
                        e.Graphics.DrawRectangle(p, 0, 0, 15, 15);
                    }
                };

                var lbl = new Label
                {
                    Text = item.Label,
                    ForeColor = Color.FromArgb(180, 195, 215),
                    Font = new Font("Segoe UI", 8.5F),
                    Location = new Point(left + 22, 3),
                    AutoSize = true
                };

                pnlLegend.Controls.Add(swatch);
                pnlLegend.Controls.Add(lbl);

                left += 120;
            }
        }

        private void InitData()
        {
            _allMovies = MovieService.GetMoviesWithShows();
            _filteredMovies = new List<Movie>(_allMovies);
            RenderMovieList();

            if (_filteredMovies.Count > 0)
            {
                SelectMovie(_filteredMovies[0]);
            }
            else
            {
                ClearMovieDetails();
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                _filteredMovies = new List<Movie>(_allMovies);
            }
            else
            {
                _filteredMovies = _allMovies
                    .Where(m => (m.Title != null && m.Title.ToLowerInvariant().Contains(query)) ||
                                (m.Genre != null && m.Genre.ToLowerInvariant().Contains(query)))
                    .ToList();
            }

            lblMoviesHeader.Text = $"NOW SHOWING ({_filteredMovies.Count})";
            RenderMovieList();

            if (_filteredMovies.Count > 0)
            {
                if (_selectedMovie == null || !_filteredMovies.Contains(_selectedMovie))
                {
                    SelectMovie(_filteredMovies[0]);
                }
            }
            else
            {
                ClearMovieDetails();
            }
        }

        private void RenderMovieList()
        {
            flowMovies.SuspendLayout();
            flowMovies.Controls.Clear();
            _movieCards.Clear();

            foreach (var m in _filteredMovies)
            {
                var card = new MovieCard();
                card.SetMovie(m);
                card.Margin = new Padding(4, 4, 4, 8);
                card.IsSelectedCard = (_selectedMovie != null && _selectedMovie.Id == m.Id);

                card.Selected += (s, e) =>
                {
                    SelectMovie(m);
                };

                _movieCards.Add(card);
                flowMovies.Controls.Add(card);
            }

            flowMovies.ResumeLayout();
        }

        private void SelectMovie(Movie m)
        {
            _selectedMovie = m;

            // Highlight selected card
            foreach (var card in _movieCards)
            {
                card.IsSelectedCard = (card.Movie != null && card.Movie.Id == m.Id);
            }

            lblTitle.Text = m.Title;
            lblPriceBadge.Text = $"🎟️ ${m.Price:F2} per ticket";
            string rel = m.ReleaseDate.HasValue ? $" • Came Out: {m.ReleaseDate.Value:yyyy-MM-dd}" : "";
            lblMeta.Text = $"{m.Genre} • {(int)m.Duration.TotalMinutes} min{rel} • Rating: {m.Rating} ({m.AgeRating})";
            lblDescription.Text = m.Description;

            // Load movie poster into picDetailPoster
            if (picDetailPoster != null)
            {
                if (!string.IsNullOrWhiteSpace(m.PosterPath))
                {
                    ImageService.LoadImageAsync(m.PosterPath, (img) =>
                    {
                        if (picDetailPoster != null && !picDetailPoster.IsDisposed)
                        {
                            picDetailPoster.Image = img ?? ImageService.CreatePlaceholder(m.Title, 95, 126);
                        }
                    }, picDetailPoster);
                }
                else
                {
                    picDetailPoster.Image = ImageService.CreatePlaceholder(m.Title, 95, 126);
                }
            }

            cbShows.Items.Clear();
            if (m.Shows != null && m.Shows.Count > 0)
            {
                foreach (var show in m.Shows)
                {
                    cbShows.Items.Add(show);
                }
                cbShows.SelectedIndex = 0;
                _selectedShow = m.Shows[0];
                RenderSeats();
            }
            else
            {
                _selectedShow = null;
                cbShows.Items.Add("No scheduled showtimes");
                cbShows.SelectedIndex = 0;
                pnlSeats.Controls.Clear();
                UpdateSelectedCount();
            }
        }

        private void ClearMovieDetails()
        {
            _selectedMovie = null;
            _selectedShow = null;
            if (picDetailPoster != null) picDetailPoster.Image = null;
            lblTitle.Text = "No movies available";
            lblPriceBadge.Text = "";
            lblMeta.Text = "";
            lblDescription.Text = "No movie found matching your search. Try another search or add a new movie.";
            cbShows.Items.Clear();
            pnlSeats.Controls.Clear();
            UpdateSelectedCount();
        }

        private void CbShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedMovie == null || _selectedMovie.Shows == null) return;
            var idx = cbShows.SelectedIndex;
            if (idx >= 0 && idx < _selectedMovie.Shows.Count)
            {
                _selectedShow = _selectedMovie.Shows[idx];
                RenderSeats();
            }
        }

        private void RefreshShowBookings(Show show)
        {
            if (show == null) return;
            MovieService.PopulateSeatsForShow(show);
        }

        private void RenderSeats()
        {
            pnlSeats.SuspendLayout();
            pnlSeats.Controls.Clear();

            if (_selectedShow == null)
            {
                pnlSeats.ResumeLayout();
                UpdateSelectedCount();
                return;
            }

            // Populate or refresh seats on-demand
            MovieService.PopulateSeatsForShow(_selectedShow);

            if (_selectedShow.Seats == null || _selectedShow.Seats.Count == 0)
            {
                pnlSeats.ResumeLayout();
                UpdateSelectedCount();
                return;
            }

            int rows = _selectedShow.TotalRows;
            int cols = _selectedShow.TotalCols;

            int seatW = 46;
            int seatH = 32;
            int gapX = 10;
            int gapY = 8;
            int startX = 40;
            int startY = 15;

            // Draw Column headers (1, 2, 3...)
            for (int c = 0; c < cols; c++)
            {
                var lblCol = new Label
                {
                    Text = (c + 1).ToString(),
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(120, 135, 155),
                    Location = new Point(startX + c * (seatW + gapX), 0),
                    Size = new Size(seatW, 14),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlSeats.Controls.Add(lblCol);
            }

            // Draw Row headers (A, B, C...) and seat buttons
            for (int r = 0; r < rows; r++)
            {
                char rowLetter = (char)('A' + r);
                var lblRow = new Label
                {
                    Text = rowLetter.ToString(),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 180, 160),
                    Location = new Point(10, startY + r * (seatH + gapY) + 6),
                    Size = new Size(24, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlSeats.Controls.Add(lblRow);

                for (int c = 0; c < cols; c++)
                {
                    var seat = _selectedShow.Seats.FirstOrDefault(s => s.Row == r && s.Number == c);
                    if (seat != null)
                    {
                        var sb = new SeatButton();
                        sb.SetSeat(seat);
                        sb.Width = seatW;
                        sb.Height = seatH;
                        sb.Left = startX + c * (seatW + gapX);
                        sb.Top = startY + r * (seatH + gapY);
                        sb.Toggled += (s, e) => UpdateSelectedCount();
                        pnlSeats.Controls.Add(sb);
                    }
                }
            }

            pnlSeats.ResumeLayout();
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            if (_selectedShow == null || _selectedMovie == null)
            {
                lblSelectedSeats.Text = "Selected Seats: None";
                lblTotalPrice.Text = "Total: $0.00";
                btnPurchase.Enabled = false;
                return;
            }

            var selectedSeats = _selectedShow.Seats.Where(s => s.IsSelected && !s.IsBooked).ToList();
            if (selectedSeats.Count == 0)
            {
                lblSelectedSeats.Text = "Selected Seats: None";
                lblTotalPrice.Text = "Total: $0.00";
                btnPurchase.Enabled = false;
            }
            else
            {
                string seatCodes = string.Join(", ", selectedSeats.Select(s => s.Label));
                decimal total = selectedSeats.Count * _selectedMovie.Price;
                lblSelectedSeats.Text = $"Selected Seats: {seatCodes} ({selectedSeats.Count} {(selectedSeats.Count == 1 ? "seat" : "seats")})";
                lblTotalPrice.Text = $"Total: ${total:F2}  ({selectedSeats.Count} × ${_selectedMovie.Price:F2})";
                btnPurchase.Enabled = true;
            }
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (_selectedShow == null || _selectedMovie == null) return;

            if (!ProgramState.IsLoggedIn)
            {
                MessageBox.Show("Please sign in to book tickets.", "Sign In Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PerformLogin();
                return;
            }

            var selectedSeats = _selectedShow.Seats.Where(s => s.IsSelected && !s.IsBooked).ToList();
            if (!selectedSeats.Any())
            {
                MessageBox.Show("Please select at least one seat to purchase.", "No Seats Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string seatCodes = string.Join(", ", selectedSeats.Select(s => s.Label));
            decimal totalAmount = selectedSeats.Count * _selectedMovie.Price;

            string confirmMsg = $"Please confirm your booking:\n\n" +
                               $"Movie: {_selectedMovie.Title}\n" +
                               $"Showtime: {_selectedShow.DisplayText}\n" +
                               $"Seats: {seatCodes} ({selectedSeats.Count} seats)\n" +
                               $"Total Price: ${totalAmount:F2}\n\n" +
                               $"Proceed with purchase?";

            var result = MessageBox.Show(confirmMsg, "Confirm Booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            string error;
            bool success = BookingService.BookSeats(ProgramState.CurrentUserId, _selectedShow.Id, selectedSeats, _selectedMovie.Price, out error);

            if (success)
            {
                foreach (var s in selectedSeats)
                {
                    s.IsBooked = true;
                    s.IsSelected = false;
                }

                RenderSeats();

                var askView = MessageBox.Show(
                    $"🎉 Booking confirmed!\n\n" +
                    $"You have successfully booked {selectedSeats.Count} ticket(s) for '{_selectedMovie.Title}'.\n" +
                    $"Seats: {seatCodes}\n" +
                    $"Total: ${totalAmount:F2}\n\n" +
                    "Would you like to open 'My Bookings' now to view or print your digital e-tickets?",
                    "Booking Confirmed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (askView == DialogResult.Yes)
                {
                    BtnMyBookings_Click(btnMyBookings, EventArgs.Empty);
                }
            }
            else
            {
                MessageBox.Show($"Failed to book seats: {error}", "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Refresh seats from DB in case another user booked them
                RefreshShowBookings(_selectedShow);
                RenderSeats();
            }
        }

        private void BtnAddMovie_Click(object sender, EventArgs e)
        {
            if (!ProgramState.CurrentUserIsAdmin)
            {
                MessageBox.Show("Only administrators can add movies.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addForm = new AddMovieForm())
            {
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload movies from database
                    _allMovies = MovieService.GetMoviesWithShows();
                    TxtSearch_TextChanged(txtSearch, EventArgs.Empty);

                    // Select the newest movie (last in list)
                    if (_allMovies.Count > 0)
                    {
                        SelectMovie(_allMovies[_allMovies.Count - 1]);
                    }
                }
            }
        }

        private void BtnMyBookings_Click(object sender, EventArgs e)
        {
            if (!ProgramState.IsLoggedIn)
            {
                MessageBox.Show("Please sign in to view your bookings.", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var myBookingsForm = new MyBookingsForm())
            {
                myBookingsForm.ShowDialog(this);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "Are you sure you want to sign out?\n\nYou will be returned to the sign-in screen to log into another account.",
                "Confirm Sign Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res != DialogResult.Yes) return;
            TriggerSignOut();
        }
    }
}

