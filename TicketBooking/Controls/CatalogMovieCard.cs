using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class CatalogMovieCard : UserControl
    {
        public Movie Movie { get; private set; }
        public event EventHandler<Movie> MovieBookClicked;

        private Panel pnlPosterBox;
        private PictureBox picPoster;
        private Label lblRating;
        private Label lblAgeRating;
        private Label lblTitle;
        private Label lblMeta;
        private Label lblShows;
        private Label lblPrice;
        private Button btnBook;
        private bool _isHovered;

        public CatalogMovieCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(224, 382);
            Margin = new Padding(10, 10, 10, 14);
            BackColor = Color.FromArgb(24, 30, 42);
            Cursor = Cursors.Hand;

            InitializeCardComponents();
            WireHoverEvents(this);
        }

        private void InitializeCardComponents()
        {
            // Poster Box Container
            pnlPosterBox = new Panel
            {
                Location = new Point(9, 9),
                Size = new Size(206, 248),
                BackColor = Color.FromArgb(16, 20, 28)
            };

            picPoster = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(16, 20, 28),
                Cursor = Cursors.Hand
            };
            picPoster.Click += (s, e) => TriggerBookClicked();

            // Overlay Badges on Poster
            lblAgeRating = new Label
            {
                Location = new Point(6, 6),
                Size = new Size(48, 20),
                BackColor = Color.FromArgb(215, 18, 24, 36),
                ForeColor = Color.FromArgb(210, 225, 245),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblAgeRating.Click += (s, e) => TriggerBookClicked();

            lblRating = new Label
            {
                Location = new Point(142, 6),
                Size = new Size(58, 20),
                BackColor = Color.FromArgb(215, 18, 24, 36),
                ForeColor = Color.FromArgb(255, 195, 60),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblRating.Click += (s, e) => TriggerBookClicked();

            pnlPosterBox.Controls.Add(lblRating);
            pnlPosterBox.Controls.Add(lblAgeRating);
            pnlPosterBox.Controls.Add(picPoster);
            Controls.Add(pnlPosterBox);

            // Movie Title
            lblTitle = new Label
            {
                Location = new Point(9, 263),
                Size = new Size(206, 22),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblTitle.Click += (s, e) => TriggerBookClicked();
            Controls.Add(lblTitle);

            // Meta Details: Genre & Duration
            lblMeta = new Label
            {
                Location = new Point(9, 287),
                Size = new Size(206, 17),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(145, 165, 190),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblMeta.Click += (s, e) => TriggerBookClicked();
            Controls.Add(lblMeta);

            // Showtimes count indicator
            lblShows = new Label
            {
                Location = new Point(9, 307),
                Size = new Size(206, 17),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblShows.Click += (s, e) => TriggerBookClicked();
            Controls.Add(lblShows);

            // Bottom Tier: Price & Book Button
            lblPrice = new Label
            {
                Location = new Point(9, 335),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 190),
                Cursor = Cursors.Hand
            };
            lblPrice.Click += (s, e) => TriggerBookClicked();
            Controls.Add(lblPrice);

            btnBook = new Button
            {
                Text = "🎟️ Book",
                Location = new Point(122, 330),
                Size = new Size(93, 32),
                BackColor = Color.FromArgb(0, 140, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Click += (s, e) => TriggerBookClicked();
            Controls.Add(btnBook);
        }

        public void SetMovie(Movie m)
        {
            Movie = m ?? throw new ArgumentNullException(nameof(m));

            lblTitle.Text = m.Title;
            lblAgeRating.Text = string.IsNullOrWhiteSpace(m.AgeRating) ? "PG" : m.AgeRating;
            lblRating.Text = string.IsNullOrWhiteSpace(m.Rating) ? "⭐ 8.5" : (m.Rating.StartsWith("⭐") ? m.Rating : "⭐ " + m.Rating.Replace("/10", ""));

            int durationMin = (int)m.Duration.TotalMinutes;
            string durText = durationMin > 0 ? $"{durationMin}m" : "120m";
            lblMeta.Text = $"{m.Genre} • {durText}";

            int showCount = m.Shows?.Count ?? 0;
            lblShows.Text = showCount > 0 ? $"🕒 {showCount} showtimes available" : "🕒 Screenings coming soon";

            lblPrice.Text = $"${m.Price:N2}";

            // Async poster loading
            ImageService.LoadImageAsync(m.PosterPath, picPoster, m.Title);

            var tip = new ToolTip();
            tip.SetToolTip(lblTitle, $"{m.Title}\n{m.Genre}\n{m.CameOutText}\nRating: {m.Rating}\n\n{m.Description}");
        }

        private void TriggerBookClicked()
        {
            if (Movie != null)
            {
                MovieBookClicked?.Invoke(this, Movie);
            }
        }

        private void WireHoverEvents(Control control)
        {
            control.MouseEnter += (s, e) => SetHoverState(true);
            control.MouseLeave += (s, e) =>
            {
                // Only clear hover if mouse has left the outer card bounds
                Point clientPt = PointToClient(Cursor.Position);
                if (!ClientRectangle.Contains(clientPt))
                {
                    SetHoverState(false);
                }
            };

            foreach (Control c in control.Controls)
            {
                WireHoverEvents(c);
            }
        }

        private void SetHoverState(bool hovered)
        {
            if (_isHovered == hovered) return;
            _isHovered = hovered;
            BackColor = _isHovered ? Color.FromArgb(32, 40, 56) : Color.FromArgb(24, 30, 42);
            btnBook.BackColor = _isHovered ? Color.FromArgb(0, 175, 150) : Color.FromArgb(0, 140, 120);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var borderRect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color borderColor = _isHovered ? Color.FromArgb(0, 210, 180) : Color.FromArgb(42, 52, 70);
            int borderWidth = _isHovered ? 2 : 1;

            using (var p = new Pen(borderColor, borderWidth))
            {
                g.DrawRectangle(p, borderRect);
            }
        }
    }
}
