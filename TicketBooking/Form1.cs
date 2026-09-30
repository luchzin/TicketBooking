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

        // Modern Two-Tier Header
        private Panel pnlSearchPill;
        private TextBox txtHeaderSearch;
        private Label lblSearchIcon;
        private const string SearchPlaceholder = "Search movies...";
        private Label lblBrandTag;
        private Button btnNavHome;
        private Button btnNavMovies;
        private Button btnNavTrending;
        private Button btnNavTopRated;
        private Button btnNavNowShowing;
        private Label lblHallsIndicator;
        private Button[] _navButtons;

        // Landing Page & Catalog Showcase
        private Panel pnlLandingPage;
        private Panel pnlCatalogFilterBar;
        private ComboBox cbCatalogGenre;
        private ComboBox cbCatalogSort;
        private FlowLayoutPanel flowCatalog;
        private Label lblCatalogCount;

        // Booking view wrapper
        private Panel pnlBookingContainer;
        private Panel pnlBookingHeader;
        private Button btnBackToCatalog;
        private Label lblBookingBreadcrumb;

        private string _activeCategory = "All";

        public Form1(bool isChildView = false)
        {
            _isChildView = isChildView;
            InitializeComponent();

            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(1000, 640);

            // Ensure database is created and seeded
            Database.EnsureCreated();

            // Enable double buffering for smooth, flicker-free rendering
            this.SetDoubleBuffered(true);
            flowMovies.SetDoubleBuffered(true);
            pnlSeats.SetDoubleBuffered(true);

            // Setup Admin Portal & Profile buttons
            SetupAdminPortalButton();
            SetupProfileButton();
            SetupModernHeader();
            SetupLandingPageAndLayout();

            topBarPanel.SetDoubleBuffered(true);
            topBarPanel.Paint += TopBarPanel_Paint;

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
            this.Shown += (s, e) => LayoutTopBarButtons();
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
            lblDescription.Location = new Point(134, 92);
            lblDescription.MaximumSize = new Size(540, 52);
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
                Size = new Size(110, 32),
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

        private void SetupModernHeader()
        {
            // Tier 1: Brand logo (left)
            lblBrand.Text = "CINETICKET";
            lblBrand.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblBrand.ForeColor = Color.FromArgb(0, 220, 195);
            lblBrand.Cursor = Cursors.Hand;
            lblBrand.AutoSize = true;
            lblBrand.Click += (s, e) => ShowLandingPage("All");

            lblBrandTag = new Label
            {
                Text = "— CINEMA —",
                Font = new Font("Segoe UI", 7F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 130, 155),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblBrandTag.Click += (s, e) => ShowLandingPage("All");
            topBarPanel.Controls.Add(lblBrandTag);

            // Tier 1: Search pill (left area)
            pnlSearchPill = new Panel
            {
                Size = new Size(220, 30),
                BackColor = Color.FromArgb(26, 34, 48),
                Cursor = Cursors.IBeam
            };
            pnlSearchPill.Paint += (s, pe) =>
            {
                var r = pnlSearchPill.ClientRectangle;
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var br = new System.Drawing.SolidBrush(Color.FromArgb(30, 40, 58)))
                    pe.Graphics.FillRectangle(br, r);
                using (var pen = new Pen(Color.FromArgb(50, 80, 110), 1))
                    pe.Graphics.DrawRectangle(pen, 0, 0, r.Width - 1, r.Height - 1);
            };

            lblSearchIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(7, 5),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            txtHeaderSearch = new TextBox
            {
                Location = new Point(28, 6),
                Size = new Size(185, 20),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(30, 40, 58),
                ForeColor = Color.FromArgb(100, 130, 155),
                BorderStyle = BorderStyle.None,
                Text = SearchPlaceholder
            };
            txtHeaderSearch.GotFocus += (s, e) =>
            {
                if (txtHeaderSearch.Text == SearchPlaceholder)
                {
                    txtHeaderSearch.Text = "";
                    txtHeaderSearch.ForeColor = Color.FromArgb(210, 225, 245);
                }
            };
            txtHeaderSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtHeaderSearch.Text))
                {
                    txtHeaderSearch.Text = SearchPlaceholder;
                    txtHeaderSearch.ForeColor = Color.FromArgb(100, 130, 155);
                }
            };
            txtHeaderSearch.TextChanged += (s, e) =>
            {
                if (txtHeaderSearch.Text != SearchPlaceholder)
                    FilterAndRenderCatalog();
            };
            txtHeaderSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ShowLandingPage();
                    FilterAndRenderCatalog();
                    e.SuppressKeyPress = true;
                }
            };

            pnlSearchPill.Controls.Add(lblSearchIcon);
            pnlSearchPill.Controls.Add(txtHeaderSearch);
            topBarPanel.Controls.Add(pnlSearchPill);

            // Tier 2: Sub-nav text links
            btnNavHome      = CreateNavButton("🏠 Home",       "All");
            btnNavMovies    = CreateNavButton("🎬 Movies",     "All");
            btnNavTrending  = CreateNavButton("🔥 Trends",     "Trending");
            btnNavTopRated  = CreateNavButton("⭐ Top Rated",  "TopRated");
            btnNavNowShowing = CreateNavButton("🕒 Now Showing", "NowShowing");

            _navButtons = new[] { btnNavHome, btnNavMovies, btnNavTrending, btnNavTopRated, btnNavNowShowing };
            foreach (var b in _navButtons)
                topBarPanel.Controls.Add(b);

            // Tier 2: Halls indicator (right)
            lblHallsIndicator = new Label
            {
                Text = "📍 All Cinemas (Dolby & IMAX) ▾",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(120, 145, 170),
                AutoSize = true,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            topBarPanel.Controls.Add(lblHallsIndicator);
        }

        private Button CreateNavButton(string text, string category)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(68, 28),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(170, 185, 205),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = category
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.MouseEnter += (s, e) =>
            {
                if ((string)btn.Tag != _activeCategory)
                    btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) =>
            {
                if ((string)btn.Tag != _activeCategory)
                    btn.ForeColor = Color.FromArgb(170, 185, 205);
            };
            btn.Click += (s, e) => ShowLandingPage(category);
            return btn;
        }

        private void UpdateNavButtonStyles()
        {
            if (_navButtons == null) return;
            foreach (var btn in _navButtons)
            {
                string cat = (string)btn.Tag;
                bool isActive = (cat == _activeCategory);
                btn.ForeColor = isActive ? Color.FromArgb(235, 45, 55) : Color.FromArgb(170, 185, 205);
                btn.Font = new Font("Segoe UI", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular);
                btn.BackColor = Color.Transparent;
            }
        }


        private void SetupLandingPageAndLayout()
        {
            // 1. Wrap existing booking panels inside pnlBookingContainer
            pnlBookingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 20, 26),
                Visible = false
            };

            pnlBookingHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(20, 26, 36),
                Padding = new Padding(12, 6, 12, 6)
            };

            btnBackToCatalog = new Button
            {
                Text = "⬅ Back to Movies / Browse",
                Location = new Point(12, 7),
                Size = new Size(195, 30),
                BackColor = Color.FromArgb(32, 44, 62),
                ForeColor = Color.FromArgb(0, 210, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBackToCatalog.FlatAppearance.BorderSize = 0;
            btnBackToCatalog.Click += (s, e) => ShowLandingPage();

            lblBookingBreadcrumb = new Label
            {
                Text = "Movies  ›  Select Showtime & Seats",
                Location = new Point(220, 13),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 180, 205),
                AutoSize = true
            };

            pnlBookingHeader.Controls.Add(btnBackToCatalog);
            pnlBookingHeader.Controls.Add(lblBookingBreadcrumb);

            Controls.Remove(rightPanel);
            Controls.Remove(leftPanel);

            pnlBookingContainer.Controls.Add(rightPanel);
            pnlBookingContainer.Controls.Add(leftPanel);
            pnlBookingContainer.Controls.Add(pnlBookingHeader);

            pnlBookingHeader.SendToBack();
            leftPanel.SendToBack();
            rightPanel.BringToFront();

            // 2. Build Landing Page Panel
            pnlLandingPage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 20, 26)
            };

            // A. Hero Banner — gradient blends header dark → body dark
            var pnlHero = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(16, 22, 32),
                Padding = new Padding(24, 10, 24, 8)
            };
            var lblHeroTitle = new Label
            {
                Text = "🎬 NOW SHOWING IN THEATRES",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 180),
                Location = new Point(24, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var lblHeroSub = new Label
            {
                Text = "Explore high-definition theatrical releases with Dolby Atmos sound and IMAX Laser projection · Select seats in real time",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 158, 180),
                Location = new Point(25, 34),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHero.Controls.Add(lblHeroTitle);
            pnlHero.Controls.Add(lblHeroSub);

            // B. Genre + Sort Filter Bar (search is now in the header pill)
            pnlCatalogFilterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(18, 24, 34),
                Padding = new Padding(16, 8, 16, 8)
            };

            cbCatalogGenre = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(16, 10),
                Size = new Size(145, 28),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(28, 36, 52),
                ForeColor = Color.White
            };
            cbCatalogGenre.Items.AddRange(new object[] {
                "All Genres",
                "Action / Adventure",
                "Sci-Fi / Adventure",
                "Animation / Family",
                "Biography / Drama",
                "Comedy / Drama",
                "Horror / Sci-Fi"
            });
            cbCatalogGenre.SelectedIndex = 0;
            cbCatalogGenre.SelectedIndexChanged += (s, e) => FilterAndRenderCatalog();

            cbCatalogSort = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(170, 10),
                Size = new Size(170, 28),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(28, 36, 52),
                ForeColor = Color.White
            };
            cbCatalogSort.Items.AddRange(new object[] {
                "🔥 Trending First",
                "⭐ Highest Rated First",
                "📅 Release Date (Newest)",
                "💲 Price (Low to High)",
                "🔤 Title (A - Z)"
            });
            cbCatalogSort.SelectedIndex = 0;
            cbCatalogSort.SelectedIndexChanged += (s, e) => FilterAndRenderCatalog();

            lblCatalogCount = new Label
            {
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlCatalogFilterBar.Resize += (s, e) =>
            {
                lblCatalogCount.Location = new Point(pnlCatalogFilterBar.ClientSize.Width - lblCatalogCount.PreferredWidth - 16, 15);
            };

            pnlCatalogFilterBar.Controls.Add(cbCatalogGenre);
            pnlCatalogFilterBar.Controls.Add(cbCatalogSort);
            pnlCatalogFilterBar.Controls.Add(lblCatalogCount);

            // C. Catalog Flow Grid
            flowCatalog = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(14, 18, 26),
                Padding = new Padding(16, 10, 16, 20)
            };
            flowCatalog.SetDoubleBuffered(true);

            pnlLandingPage.Controls.Add(flowCatalog);
            pnlLandingPage.Controls.Add(pnlCatalogFilterBar);
            pnlLandingPage.Controls.Add(pnlHero);

            // Gradient paint on hero: blends header bottom-color into page color
            pnlHero.Paint += (s, pe) =>
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlHero.ClientRectangle,
                    Color.FromArgb(18, 24, 34),
                    Color.FromArgb(18, 24, 34),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    pe.Graphics.FillRectangle(br, pnlHero.ClientRectangle);

                // Re-draw the children text manually is not needed; just the bg gradient
            };

            Controls.Add(pnlBookingContainer);
            Controls.Add(pnlLandingPage);

            topBarPanel.SendToBack();
            pnlLandingPage.BringToFront();
        }





        public void OpenBookingForMovie(Movie m)
        {
            if (m == null) return;
            SelectMovie(m);
            if (lblBookingBreadcrumb != null)
            {
                lblBookingBreadcrumb.Text = $"Movies  ›  {m.Title}  ›  Choose Showtime & Seats";
            }
            pnlLandingPage.Visible = false;
            pnlBookingContainer.Visible = true;
            pnlBookingContainer.BringToFront();
        }

        public void ShowLandingPage(string category = null)
        {
            pnlBookingContainer.Visible = false;
            pnlLandingPage.Visible = true;
            pnlLandingPage.BringToFront();

            if (category != null)
            {
                _activeCategory = category;
                UpdateNavButtonStyles();
                FilterAndRenderCatalog();
            }
        }

        private void FilterAndRenderCatalog()
        {
            if (_allMovies == null || flowCatalog == null) return;

            string rawSearch = txtHeaderSearch?.Text ?? "";
            string search = (rawSearch == SearchPlaceholder ? "" : rawSearch).Trim().ToLowerInvariant();
            string selectedGenre = cbCatalogGenre?.SelectedItem?.ToString() ?? "All Genres";
            string selectedSort = cbCatalogSort?.SelectedItem?.ToString() ?? "🔥 Trending First";

            var result = _allMovies.AsEnumerable();

            // 1. Category Pill Filter
            if (_activeCategory == "Trending")
            {
                result = result.Where(m => ParseNumericRating(m.Rating) >= 8.6 || (m.Shows?.Count ?? 0) >= 5);
            }
            else if (_activeCategory == "TopRated")
            {
                result = result.Where(m => ParseNumericRating(m.Rating) >= 8.5);
            }
            else if (_activeCategory == "NowShowing")
            {
                result = result.Where(m => (m.Shows?.Count ?? 0) > 0);
            }
            else if (_activeCategory == "Action")
            {
                result = result.Where(m => m.Genre != null && (m.Genre.IndexOf("Action", StringComparison.OrdinalIgnoreCase) >= 0 || m.Genre.IndexOf("Sci-Fi", StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // 2. Genre Dropdown
            if (selectedGenre != "All Genres" && !string.IsNullOrWhiteSpace(selectedGenre))
            {
                result = result.Where(m => m.Genre != null && m.Genre.IndexOf(selectedGenre.Split('/')[0].Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 3. Search Text
            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(m =>
                    (m.Title != null && m.Title.ToLowerInvariant().Contains(search)) ||
                    (m.Genre != null && m.Genre.ToLowerInvariant().Contains(search)) ||
                    (m.Description != null && m.Description.ToLowerInvariant().Contains(search))
                );
            }

            // 4. Sort
            if (selectedSort.Contains("Trending"))
            {
                result = result.OrderByDescending(m => ParseNumericRating(m.Rating))
                               .ThenByDescending(m => m.Shows?.Count ?? 0);
            }
            else if (selectedSort.Contains("Rated") || selectedSort.Contains("Highest"))
            {
                result = result.OrderByDescending(m => ParseNumericRating(m.Rating));
            }
            else if (selectedSort.Contains("Release") || selectedSort.Contains("Newest"))
            {
                result = result.OrderByDescending(m => m.ReleaseDate ?? DateTime.MinValue);
            }
            else if (selectedSort.Contains("Price"))
            {
                result = result.OrderBy(m => m.Price);
            }
            else if (selectedSort.Contains("Title") || selectedSort.Contains("A - Z"))
            {
                result = result.OrderBy(m => m.Title);
            }

            var list = result.ToList();

            flowCatalog.SuspendLayout();
            flowCatalog.Controls.Clear();

            foreach (var movie in list)
            {
                var card = new CatalogMovieCard();
                card.SetMovie(movie);
                card.MovieBookClicked += (s, m) => OpenBookingForMovie(m);
                flowCatalog.Controls.Add(card);
            }

            flowCatalog.ResumeLayout();

            if (lblCatalogCount != null)
            {
                lblCatalogCount.Text = $"Showing {list.Count} of {_allMovies.Count} Movies";
                if (pnlCatalogFilterBar != null)
                {
                    lblCatalogCount.Location = new Point(pnlCatalogFilterBar.ClientSize.Width - lblCatalogCount.PreferredWidth - 16, 17);
                }
            }
        }

        private double ParseNumericRating(string rating)
        {
            if (string.IsNullOrWhiteSpace(rating)) return 8.0;
            string clean = rating.Replace("⭐", "").Trim();
            if (clean.Contains("/")) clean = clean.Split('/')[0].Trim();
            if (double.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                return val;
            }
            return 8.0;
        }

        private void LayoutTopBarButtons()
        {
            if (topBarPanel == null) return;

            int W = topBarPanel.ClientSize.Width;

            // ── TIER 1 (y: 0 → 45) ─────────────────────────────────────────────

            // Left: Brand stacked logo
            int logoX = 18;
            if (lblBrand != null)
            {
                lblBrand.Location = new Point(logoX, 8);
                lblBrand.BringToFront();
            }
            if (lblBrandTag != null)
            {
                lblBrandTag.Location = new Point(logoX + 2, 28);
                lblBrandTag.BringToFront();
            }

            // Left-center: Search pill (starts after brand)
            int searchX = logoX + 120;
            if (pnlSearchPill != null)
            {
                pnlSearchPill.Location = new Point(searchX, 8);
                pnlSearchPill.BringToFront();
            }

            if (lblUserInfo != null) lblUserInfo.Visible = false;

            if (ProgramState.IsLoggedIn)
            {
                // Right: action pill buttons (right → left)
                int btnY1 = 9;
                int right = W - 14;

                if (btnLogout != null)
                {
                    btnLogout.Size = new Size(90, 28);
                    btnLogout.Location = new Point(right - btnLogout.Width, btnY1);
                    btnLogout.BringToFront();
                    right = btnLogout.Left - 6;
                }
                if (btnProfile != null)
                {
                    int mw = TextRenderer.MeasureText(btnProfile.Text, btnProfile.Font).Width + 26;
                    btnProfile.Width = Math.Max(100, Math.Min(180, mw));
                    btnProfile.Height = 28;
                    btnProfile.Location = new Point(right - btnProfile.Width, btnY1);
                    btnProfile.BringToFront();
                    right = btnProfile.Left - 6;
                }
                if (btnMyBookings != null)
                {
                    btnMyBookings.Size = new Size(120, 28);
                    btnMyBookings.Location = new Point(right - btnMyBookings.Width, btnY1);
                    btnMyBookings.BringToFront();
                    right = btnMyBookings.Left - 6;
                }
                bool isAdmin = ProgramState.CurrentUserIsAdmin;
                if (btnAdminPortal != null && isAdmin)
                {
                    btnAdminPortal.Size = new Size(128, 28);
                    btnAdminPortal.Location = new Point(right - btnAdminPortal.Width, btnY1);
                    btnAdminPortal.BringToFront();
                    right = btnAdminPortal.Left - 6;
                }
            }

            // ── TIER 2 (y: 50 → 82) ────────────────────────────────────────────

            int navY = 52;
            int navX = 18;
            if (_navButtons != null)
            {
                foreach (var b in _navButtons)
                {
                    if (b == null) continue;
                    b.Height = 26;
                    b.Location = new Point(navX, navY);
                    b.BringToFront();
                    navX += b.Width + 2;
                }
            }

            // Halls indicator (right of Tier 2)
            if (lblHallsIndicator != null)
            {
                lblHallsIndicator.Location = new Point(W - lblHallsIndicator.PreferredWidth - 14, navY + 5);
                lblHallsIndicator.BringToFront();
            }
        }

        private void TopBarPanel_Paint(object sender, PaintEventArgs e)
        {
            var r = topBarPanel.ClientRectangle;
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                r,
                Color.FromArgb(10, 14, 22),
                Color.FromArgb(16, 22, 32),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(br, r);
            }
            // Draw the subtle tier-divider line at y=46
            using (var pen = new Pen(Color.FromArgb(30, 50, 70), 1))
                e.Graphics.DrawLine(pen, 0, 46, r.Width, 46);
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
            Close();
        }

        private void UpdateUserSessionUi()
        {
            if (ProgramState.IsLoggedIn)
            {
                bool isAdmin = ProgramState.CurrentUserIsAdmin;
                string displayName = string.IsNullOrWhiteSpace(ProgramState.CurrentUserFullName)
                    ? ProgramState.CurrentUserPhone
                    : ProgramState.CurrentUserFullName;

                if (btnProfile != null)
                {
                    btnProfile.Text = isAdmin ? $"👑 {displayName}" : $"👤 {displayName}";
                    int measuredW = TextRenderer.MeasureText(btnProfile.Text, btnProfile.Font).Width + 24;
                    btnProfile.Width = Math.Max(100, Math.Min(175, measuredW));
                    btnProfile.ForeColor = isAdmin ? Color.FromArgb(255, 215, 80) : Color.FromArgb(200, 225, 255);
                    btnProfile.Visible = true;
                }

                if (lblUserInfo != null) lblUserInfo.Visible = false;
                if (btnAddMovie != null) btnAddMovie.Visible = false;
                if (btnAdminPortal != null) btnAdminPortal.Visible = isAdmin;
                if (btnEditMovieDetail != null) btnEditMovieDetail.Visible = isAdmin;
                if (btnDeleteMovieDetail != null) btnDeleteMovieDetail.Visible = isAdmin;
                if (btnAddShowDetail != null) btnAddShowDetail.Visible = isAdmin;
                btnMyBookings.Visible = true;
                btnLogout.Visible = true;
            }
            else
            {
                if (btnProfile != null) btnProfile.Visible = false;
                if (lblUserInfo != null) lblUserInfo.Visible = false;
                if (btnAddMovie != null) btnAddMovie.Visible = false;
                if (btnAdminPortal != null) btnAdminPortal.Visible = false;
                if (btnEditMovieDetail != null) btnEditMovieDetail.Visible = false;
                if (btnDeleteMovieDetail != null) btnDeleteMovieDetail.Visible = false;
                if (btnAddShowDetail != null) btnAddShowDetail.Visible = false;
                btnMyBookings.Visible = false;
                btnLogout.Visible = false;
            }

            LayoutTopBarButtons();
            UpdateNavButtonStyles();
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
            FilterAndRenderCatalog();

            if (_filteredMovies.Count > 0)
            {
                SelectMovie(_filteredMovies[0]);
            }
            else
            {
                ClearMovieDetails();
            }

            if (!_isChildView)
            {
                ShowLandingPage("All");
            }
            else
            {
                pnlLandingPage.Visible = false;
                pnlBookingContainer.Visible = true;
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

            if (ProgramState.CurrentUserIsAdmin)
            {
                MessageBox.Show("Administrators cannot book tickets. Please use the Admin Portal for cinema management.", "Admin Restriction", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

